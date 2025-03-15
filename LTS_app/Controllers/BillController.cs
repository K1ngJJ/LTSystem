using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Admin,Legislator")]
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bill bill)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data. Please check the form inputs.";
                return RedirectToAction(nameof(Index));
            }

            // 1️⃣ Get the logged-in user's username from authentication claims
            var username = User?.Identity?.Name; // This retrieves the username

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "User authentication failed.";
                return RedirectToAction(nameof(Index));
            }

            // 2️⃣ Retrieve the user from the database using the username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unauthorized: User not found.";
                return RedirectToAction(nameof(Index));
            }

            // 3️⃣ Assign UserId before saving
            bill.UserId = user.Id;
            bill.IntroducedDate = DateTime.UtcNow;
            bill.CreatedAt = DateTime.UtcNow;

            try
            {
                // 4️⃣ Save the bill
                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bill created successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving bill: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while saving the bill.";
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bill bill)
        {
            if (id != bill.Id)
            {
                TempData["ErrorMessage"] = "Invalid bill ID.";
                return RedirectToAction(nameof(Index));
            }

            // Validate if all required fields are provided
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

            // Update fields
            existingBill.Title = bill.Title;
            existingBill.Description = bill.Description;
            existingBill.CommitteeId = bill.CommitteeId;
            existingBill.SessionId = bill.SessionId;
            existingBill.Status = bill.Status;

            // Set IntroducedDate to current UTC date when updating the bill
            existingBill.IntroducedDate = DateTime.UtcNow;

            try
            {
                _context.Update(existingBill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bill updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the bill. " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }



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
                committee = bill.Committee?.Name ?? "N/A",
                session = bill.Session?.Name ?? "N/A",
                introducedDate = bill.IntroducedDate
            };

            return Json(result);
        }


    }
}







