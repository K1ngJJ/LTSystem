using System.Diagnostics;
using LTS_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace LTS_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ✅ Redirect users based on role using Claims
        private IActionResult RedirectToDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Legislator" => RedirectToAction("Dashboard", "Legislator"),
                _ => RedirectToAction("Dashboard", "User")
            };
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated) // ✅ Check authentication via Identity
            {
                return RedirectToDashboard();
            }
            return View();
        }

        public IActionResult Privacy()
        {
            if (User.Identity.IsAuthenticated) // ✅ Check authentication via Identity
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
