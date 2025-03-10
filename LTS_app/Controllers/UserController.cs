using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

namespace LTS_app.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UserController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Authorize(Roles = "User")]
        public IActionResult Dashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        private bool IsAdmin()
        {
            return User.Identity.IsAuthenticated && User.IsInRole("Admin");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return string.Concat(bytes.Select(b => b.ToString("x2")));
            }
        }

        private string GenerateToken()
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(tokenBytes);
        }

        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public IActionResult Create(string username, string email, string password, string role, string fullName)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            var validRoles = new[] { "Admin", "Legislator", "User" };
            if (!validRoles.Contains(role))
            {
                ModelState.AddModelError("", "Invalid role.");
                return View();
            }

            if (_context.Users.Any(u => u.Username == username) || _context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Username or email already taken.");
                return View();
            }

            string hashedPassword = HashPassword(password);
            string confirmationToken = GenerateToken();

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                Role = role,
                FullName = fullName, // Add FullName field here
                Token = GenerateToken(),
                IsConfirmed = true,
                IsActive = false,
                ConfirmationToken = confirmationToken,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            if (role != "Admin")
            {
                SendVerificationEmail(email, confirmationToken);
            }

            return RedirectToAction("Index");
        }

        private void SendVerificationEmail(string email, string token)
        {
            var smtpUser = _config["EmailSettings:SmtpUser"];
            var smtpPassword = _config["EmailSettings:SmtpPassword"];
            var smtpHost = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);

            var confirmationLink = Url.Action("VerifyEmail", "User", new { token }, Request.Scheme);
            var fromAddress = new MailAddress(smtpUser, "LTS App");
            var toAddress = new MailAddress(email);
            const string subject = "Email Verification - LTS App";
            string body = $"Click the link to verify your account: <a href='{confirmationLink}'>Verify Email</a>";

            using (var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
        }

        public IActionResult VerifyEmail(string token)
        {
            var user = _context.Users.FirstOrDefault(u => (u.ConfirmationToken ?? "").Trim() == token.Trim());

            if (user == null) return NotFound("Invalid verification token.");

            user.IsConfirmed = true;
            user.IsActive = true;
            user.ConfirmationToken = null;

            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["Message"] = "Your email has been verified successfully! You can now log in.";

            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View(_context.Users.ToList());
        }

        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(int id, string username, string email, string role, string fullName)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Username = username;
            user.Email = email;
            user.Role = role;
            user.FullName = fullName; // Update FullName here
            user.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ToggleStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}
