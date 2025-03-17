using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "User")]
    public class CitizenFeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitizenFeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int BillId, string FeedbackText)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (string.IsNullOrWhiteSpace(FeedbackText))
            {
                TempData["ErrorMessage"] = "Feedback cannot be empty.";
                return RedirectToAction("Index", "Bill");
            }

            var feedback = new CitizenFeedback
            {
                BillId = BillId,
                UserId = userId,
                FeedbackText = FeedbackText,
                SubmittedAt = DateTime.UtcNow
            };

            _context.CitizenFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Feedback submitted successfully!";
            return RedirectToAction("Details", "Bill", new { id = BillId });
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedbacks(int billId)
        {
            var feedbacks = await _context.CitizenFeedbacks
                .Where(f => f.BillId == billId)
                .Include(f => f.User)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new
                {
                    UserName = f.User.FullName,
                    FeedbackText = f.FeedbackText,
                    SubmittedAt = f.SubmittedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(feedbacks);
        }
    }
}
