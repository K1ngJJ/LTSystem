using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Legislator")] // Only legislators can create/amend bills
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all bills
        public async Task<IActionResult> Index()
        {
            var bills = await _context.Bills
                .Include(b => b.Legislator)
                .Include(b => b.Committee)
                .Include(b => b.BillHistories)
                .ToListAsync();
            return View(bills);
        }

        // Show create bill form
        public IActionResult Create()
        {
            ViewBag.Committees = _context.Committees.ToList();
            ViewBag.Sessions = _context.Sessions.ToList();
            return View();
        }

        // Save new bill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bill bill)
        {
            if (ModelState.IsValid)
            {
                bill.LegislatorId = int.Parse(User.Identity.Name); // Assuming Legislator ID is stored in Identity
                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bill);
        }

        // Edit bill
        public async Task<IActionResult> Edit(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
                return NotFound();

            ViewBag.Committees = _context.Committees.ToList();
            ViewBag.Sessions = _context.Sessions.ToList();
            return View(bill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bill bill)
        {
            if (id != bill.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bill);
        }

        // Delete bill
        public async Task<IActionResult> Delete(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
                return NotFound();

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Assign bill to committee
        [HttpPost]
        public async Task<IActionResult> AssignToCommittee(int billId, int committeeId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return NotFound();

            bill.CommitteeId = committeeId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Change bill status
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int billId, string status)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return NotFound();

            _context.BillHistories.Add(new BillHistory
            {
                BillId = billId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
