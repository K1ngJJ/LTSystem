using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Admin,Legislator,User")]
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                ViewBag.UserName = user.FullName; // Set ViewBag to show user's name in the form
            }

            var bills = await _context.Bills
                .Include(b => b.User)
                .Include(b => b.Committee)
                .Include(b => b.Session)
                .ToListAsync();

            var committees = await _context.Committees.ToListAsync();
            var sessions = await _context.Sessions.ToListAsync();

            ViewBag.Committees = committees.Any()
                ? new SelectList(committees, "Id", "Name")
                : new SelectList(new List<Committee>());

            ViewBag.Sessions = sessions.Any()
                ? new SelectList(sessions, "Id", "Name")
                : new SelectList(new List<Session>());

            return View(bills);
        }



        [Authorize(Roles = "Legislator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Title, string Description, int CommitteeId, int SessionId, string Status)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (string.IsNullOrEmpty(Title) || CommitteeId == 0 || SessionId == 0)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Index));
            }
            var createbill = new Bill
            {
                UserId = userId,
                Title = Title,
                Description = Description,
                CommitteeId = CommitteeId,
                SessionId = SessionId,
                Status = Status,
                IntroducedDate = DateTime.Now
            };

            _context.Bills.Add(createbill);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // ✅ GET: Fetch Dropdown Data for Modal
        [HttpGet]
        public IActionResult GetDropdownData()
        {

            var committees = _context.Committees
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            var sessions = _context.Sessions
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();

            return Json(new { committees, sessions });
        }

        [Authorize(Roles = "Legislator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bill bill)
        {
            // Get the logged-in user's ID from claims
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (id != bill.Id)
            {
                TempData["ErrorMessage"] = "Invalid bill ID.";
                return RedirectToAction(nameof(Index));
            }

            // Validate required fields
            if (bill.CommitteeId == 0 || bill.SessionId == 0 || string.IsNullOrEmpty(bill.Title))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Index));
            }

            var existingBill = await _context.Bills.FindAsync(id);
            if (existingBill == null)
            {
                TempData["ErrorMessage"] = "Bill not found.";
                return RedirectToAction(nameof(Index));
            }

            // Ensure the logged-in user is the owner of the bill
            if (existingBill.UserId != userId)
            {
                TempData["ErrorMessage"] = "Unauthorized: You do not have permission to edit this bill.";
                return RedirectToAction(nameof(Index));
            }

            // Update fields
            existingBill.Title = bill.Title;
            existingBill.Description = bill.Description;
            existingBill.CommitteeId = bill.CommitteeId;
            existingBill.SessionId = bill.SessionId;
            existingBill.Status = bill.Status;

            // Update last modified time
            existingBill.IntroducedDate = DateTime.UtcNow;

            try
            {
                _context.Bills.Update(existingBill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bill updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the bill. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = "Legislator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                TempData["ErrorMessage"] = "Bill not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bill deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the bill.";
                Console.WriteLine($"Error deleting bill: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> GetBillDetails(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.User)
                .Include(b => b.Committee)
                .Include(b => b.Session)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
                return NotFound();

            var result = new
            {
                title = bill.Title,
                description = bill.Description,
                status = bill.Status,
                proposedBy = bill.User?.FullName ?? "Unknown",
                committee = bill.Committee?.Name ?? "N/A",
                session = bill.Session?.Name ?? "N/A",
                introducedDate = bill.IntroducedDate
            };

            return Json(result);
        }

    }
}







