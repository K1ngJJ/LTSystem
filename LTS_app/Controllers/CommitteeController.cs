using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Legislator,Admin")] // Restrict access to Legislators & Admins
    public class CommitteeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommitteeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ List all committees - Async for better performance
        public async Task<IActionResult> Index()
        {
            // Get all legislators who are NOT assigned to any committee
            var unassignedLegislators = await _context.Legislators
                .Where(l => !_context.CommitteeLegislators.Any(cl => cl.LegislatorId == l.Id))
                .Include(l => l.User) // Include User details for displaying full name
                .ToListAsync();

            ViewBag.Legislators = unassignedLegislators; // Send only unassigned legislators to the view
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name, string Description, List<int>? LegislatorIds, List<string>? Positions)
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Description))
            {
                TempData["ErrorMessage"] = "Please fill all required fields.";
                return RedirectToAction(nameof(Index));
            }

            // Create new committee
            var newCommittee = new Committee
            {
                Name = Name,
                Description = Description
            };

            _context.Committees.Add(newCommittee);
            await _context.SaveChangesAsync(); // Save committee first

            // Check if LegislatorIds is provided and not empty before assigning legislators
            if (LegislatorIds != null && LegislatorIds.Any())
            {
                // Check if any legislator is already assigned to another committee
                var existingLegislators = await _context.CommitteeLegislators
                    .Where(cl => LegislatorIds.Contains(cl.LegislatorId))
                    .Select(cl => cl.LegislatorId)
                    .ToListAsync();

                if (existingLegislators.Any())
                {
                    TempData["ErrorMessage"] = "One or more selected legislators are already assigned to another committee.";
                    return RedirectToAction(nameof(Index));
                }

                // Assign selected legislators to the new committee
                foreach (var legislatorId in LegislatorIds)
                {
                    var committeeLegislator = new CommitteeLegislator
                    {
                        CommitteeId = newCommittee.Id,
                        LegislatorId = legislatorId,
                    };

                    _context.CommitteeLegislators.Add(committeeLegislator);
                }

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Committee created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Name, string Description, List<int>? LegislatorIds)
        {
            var committee = await _context.Committees
                .Include(c => c.CommitteeLegislators)
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (committee == null)
                return NotFound();

            committee.Name = Name;
            committee.Description = Description;

            // Get current legislator IDs assigned to this committee
            var existingLegislatorIds = committee.CommitteeLegislators.Select(cl => cl.LegislatorId).ToList();

            if (LegislatorIds != null)
            {
                // Add new legislators that are not already assigned
                foreach (var legislatorId in LegislatorIds)
                {
                    if (!existingLegislatorIds.Contains(legislatorId))
                    {
                        _context.CommitteeLegislators.Add(new CommitteeLegislator
                        {
                            CommitteeId = committee.Id,
                            LegislatorId = legislatorId
                        });
                    }
                }
            }
            else
            {
                // If no LegislatorIds are provided, remove all members
                _context.CommitteeLegislators.RemoveRange(committee.CommitteeLegislators);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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

        [HttpGet]
        public async Task<IActionResult> GetCommitteeMembers(int id)
        {
            var members = await _context.CommitteeLegislators
                .Where(cl => cl.CommitteeId == id)
                .Include(cl => cl.Legislator)
                .ThenInclude(l => l.User)
                .Select(cl => new
                {
                    CommitteeId = cl.CommitteeId,  // ✅ Use CommitteeId
                    LegislatorId = cl.LegislatorId,  // ✅ Use LegislatorId
                    FullName = cl.Legislator.User != null ? cl.Legislator.User.FullName : "N/A",
                    Position = !string.IsNullOrEmpty(cl.Legislator.Position) ? cl.Legislator.Position : "Unknown"
                })
                .ToListAsync();

            return Json(members);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveCommitteeMember(int committeeId, int legislatorId)
        {
            var committeeLegislator = await _context.CommitteeLegislators
                .FirstOrDefaultAsync(cl => cl.CommitteeId == committeeId && cl.LegislatorId == legislatorId);

            if (committeeLegislator == null)
                return NotFound();

            _context.CommitteeLegislators.Remove(committeeLegislator);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}