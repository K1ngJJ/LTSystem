using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Admin")] // Ensure only Admins can access
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}
