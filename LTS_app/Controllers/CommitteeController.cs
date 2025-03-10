﻿using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Legislator,Admin")] // Restrict access to Legislators & Admins
    [Route("Committee")]
    public class CommitteeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommitteeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ List all committees - Async for better performance
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var committees = await _context.Committees.ToListAsync();
            return View(committees);
        }

        // ✅ View Committee Details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var committee = await _context.Committees
                .Include(c => c.Bills) // Assuming Committee has a collection of Bills
                .FirstOrDefaultAsync(c => c.Id == id);

            if (committee == null)
                return NotFound();

            return View(committee);
        }

        // ✅ Admin only: Show form to create a new committee
        [Authorize(Roles = "Admin")]
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Admin only: Save new committee
        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Committee committee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(committee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(committee);
        }

        // ✅ Admin only: Show edit form
        [Authorize(Roles = "Admin")]
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var committee = await _context.Committees.FindAsync(id);
            if (committee == null)
                return NotFound();

            return View(committee);
        }

        // ✅ Admin only: Update committee details
        [Authorize(Roles = "Admin")]
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Committee committee)
        {
            if (id != committee.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(committee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(committee);
        }

        // ✅ Admin only: Delete committee
        [Authorize(Roles = "Admin")]
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var committee = await _context.Committees.FindAsync(id);
            if (committee == null)
                return NotFound();

            _context.Committees.Remove(committee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ✅ Legislators & Admins can assign bills to committees
        [Authorize(Roles = "Legislator,Admin")]
        [HttpPost("AssignBill")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignBill(int billId, int committeeId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            var committee = await _context.Committees.FindAsync(committeeId);

            if (bill == null || committee == null)
                return NotFound();

            bill.CommitteeId = committeeId;
            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), "Bill");
        }
    }
}
