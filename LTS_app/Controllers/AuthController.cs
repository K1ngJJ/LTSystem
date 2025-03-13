using LTS_app.Data;
using LTS_app.Models;
using LTS_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LTS_app.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserLogService _userLogService;

        public AuthController(IAuthService authService, IUserLogService userLogService)
        {
            _authService = authService;
            _userLogService = userLogService;
        }

        [Authorize] // ✅ Ensures only authenticated users can access this action
        public IActionResult Dashboard()
        {
            return View(); // ✅ No need to manually check authentication, [Authorize] does it
        }

        private IActionResult RedirectToDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value; // ✅ Get role from claims

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Legislator" => RedirectToAction("Dashboard", "Legislator"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _authService.AuthenticateUserAsync(username, password);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                new Claim(ClaimTypes.Name, result.User.Username),
                new Claim(ClaimTypes.Role, result.User.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // 🔹 Log User Login Activity
            await _userLogService.LogActivityAsync(
                result.User.Id,
                result.User.Username,
                result.User.FullName, // ✅ Full Name included
                "Login",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return result.User.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Legislator" => RedirectToAction("Dashboard", "Legislator"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string role = "User", string fullName = "")
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError("", "Full Name is required.");
                return View();
            }

            var result = await _authService.RegisterUserAsync(username, email, password, role, fullName);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            // ✅ Log user registration activity
            await _userLogService.LogActivityAsync(
                result.User!.Id,  // ✅ Fix: Now result.User is always available
                result.User.Username,
                result.User.FullName,
                "Register",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            // Show success message & ask user to verify email
            ViewBag.Message = "Registration successful! Please check your email to verify your account.";
            return View();
        }

        public async Task<IActionResult> VerifyEmail(string token)
        {
            var result = await _authService.VerifyEmailAsync(token);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Login");
            }

            TempData["Message"] = result.Message;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var result = await _authService.RequestPasswordResetAsync(email);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            ViewBag.Message = result.Message;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var result = await _authService.ResetPasswordAsync(token, newPassword);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(new { token });
            }

            // 🔹 Log User Reset Password Activity
            var user = await _authService.GetUserByTokenAsync(token);
            if (user != null)
            {
                await _userLogService.LogActivityAsync(
                    user.Id,
                    user.Username,
                    user.FullName, // ✅ Full Name included
                    "Reset Password",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
            }

            ViewBag.Message = result.Message;
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            // Get User ID before signing out
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var fullName = User.FindFirst("FullName")?.Value; // Retrieve Full Name from Claims if added

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 🔹 Log User Logout Activity
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username))
            {
                await _userLogService.LogActivityAsync(
                    int.Parse(userId),
                    username,
                    fullName ?? "", // ✅ Full Name included if available
                    "Logout",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}
