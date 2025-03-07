using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Legislator")] // Ensure only Admins can access
    public class LegislatorController : Controller
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
