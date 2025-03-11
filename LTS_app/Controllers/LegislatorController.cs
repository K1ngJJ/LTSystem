using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize]
    public class LegislatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LegislatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Legislator Dashboard
        [Authorize(Roles = "Legislator")]
        public IActionResult Dashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // List All Legislators (Admins Only)
        [Authorize(Roles = "Admin")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var legislators = await _context.Legislators
                                            .Include(l => l.User)
                                            .ToListAsync();

            var usersWithRoleLegislator = await _context.Users
                .Where(u => u.Role == "Legislator" && !_context.Legislators.Any(l => l.UserId == u.Id))
                .ToListAsync();

            ViewBag.Users = usersWithRoleLegislator;
            return View(legislators);
        }

        // Create Legislator (Admins Only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int UserId, string Position)
        {
            if (UserId == 0 || string.IsNullOrEmpty(Position))
            {
                TempData["ErrorMessage"] = "Please select a valid user and enter a position.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Selected user not found.";
                return RedirectToAction(nameof(Index));
            }

            var newLegislator = new Legislator
            {
                UserId = UserId,
                Position = Position
            };

            _context.Legislators.Add(newLegislator);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Legislator created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Edit Legislator (Admins Only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string FullName, string Position)
        {
            var legislator = await _context.Legislators.Include(l => l.User).FirstOrDefaultAsync(l => l.Id == Id);
            if (legislator == null)
            {
                TempData["ErrorMessage"] = "Legislator not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update Position
            legislator.Position = Position;

            // Update Full Name (If Needed)
            if (!string.IsNullOrEmpty(FullName) && legislator.User != null)
            {
                legislator.User.FullName = FullName;
                _context.Users.Update(legislator.User);
            }

            _context.Legislators.Update(legislator);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Legislator updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Delete Legislator (Admins Only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var legislator = await _context.Legislators.FindAsync(id);
            if (legislator != null)
            {
                _context.Legislators.Remove(legislator);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Legislator deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Legislator not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
