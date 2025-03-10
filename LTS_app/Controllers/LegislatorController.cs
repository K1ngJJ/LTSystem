using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    public class LegislatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LegislatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ensure only Legislators have access to the dashboard
        [Authorize(Roles = "Legislator")]
        public IActionResult Dashboard()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }



        // GET: Legislator/Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {

            var legislators = await _context.Legislators
                                           .Include(l => l.User)
                                           .ToListAsync();
            return View(legislators);
        }

        // GET: Legislator/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // Fetch all users with the "Legislator" role to populate the dropdown
            var usersWithRoleLegislator = _context.Users
                .Where(u => u.Role == "Legislator")
                .ToList();

            ViewBag.Users = usersWithRoleLegislator;

            return View();
        }

        // POST: Legislator/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Legislator legislator)
        {
            if (ModelState.IsValid)
            {
                _context.Add(legislator);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Legislator created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(legislator);
        }

        // DELETE: Legislator/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var legislator = await _context.Legislators.FindAsync(id);
            if (legislator != null)
            {
                _context.Legislators.Remove(legislator);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Legislator deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
