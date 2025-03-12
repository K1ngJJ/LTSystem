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
            var bills = await _context.Bills
                .Include(b => b.Legislator)
                    .ThenInclude(l => l.User)
                .Include(b => b.Committee)
                .Include(b => b.Session)
                .ToListAsync();

            var legislators = await _context.Legislators
                .Include(l => l.User)
                .Where(l => l.User != null)  // Ensure User exists
                .ToListAsync();

            var committees = await _context.Committees.ToListAsync();
            var sessions = await _context.Sessions.ToListAsync();

            ViewBag.Legislators = legislators.Any()
                ? new SelectList(legislators, "Id", "User.FullName")
                : new SelectList(new List<Legislator>());

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
            if (bill.LegislatorId == 0 || bill.CommitteeId == 0 || bill.SessionId == 0 || string.IsNullOrEmpty(bill.Title))
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Index));
            }

            var legislator = await _context.Legislators.FindAsync(bill.LegislatorId);
            if (legislator == null)
            {
                TempData["ErrorMessage"] = "Selected legislator not found.";
                return RedirectToAction(nameof(Index));
            }

            bill.IntroducedDate = DateTime.UtcNow; // Set current date when creating a bill

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bill created successfully!";
            return RedirectToAction(nameof(Index));
        }



        // ✅ GET: Fetch Dropdown Data for Modal
        [HttpGet]
        public IActionResult GetDropdownData()
        {
            var legislators = _context.Legislators
                .Include(l => l.User)
                .Select(l => new { id = l.Id, name = l.User.FullName })
                .ToList();

            var committees = _context.Committees
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            var sessions = _context.Sessions
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();

            return Json(new { legislators, committees, sessions });
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
            if (bill.LegislatorId == 0 || bill.CommitteeId == 0 || bill.SessionId == 0 || string.IsNullOrEmpty(bill.Title))
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
            existingBill.LegislatorId = bill.LegislatorId;
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
                .Include(b => b.Legislator).ThenInclude(l => l.User)
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
                legislator = bill.Legislator?.User?.FullName ?? "N/A",
                committee = bill.Committee?.Name ?? "N/A",
                session = bill.Session?.Name ?? "N/A",
                introducedDate = bill.IntroducedDate
            };

            return Json(result);
        }


    }
}
