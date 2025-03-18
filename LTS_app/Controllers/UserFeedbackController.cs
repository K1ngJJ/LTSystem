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
    [Authorize(Roles = "Admin,Legislator,User")]
    public class UserFeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserFeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(int BillId, string FeedbackText)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (string.IsNullOrWhiteSpace(FeedbackText))
            {
                TempData["ErrorMessage"] = "Feedback cannot be empty.";
                return RedirectToAction(nameof(Index), "Bill"); // Stay on Bill Index
            }

            var feedback = new UserFeedback
            {
                BillId = BillId,
                UserId = userId,
                FeedbackText = FeedbackText,
                SubmittedAt = DateTime.Now
            };

            _context.UserFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Feedback submitted successfully!";
            return RedirectToAction(nameof(Index), "Bill"); // ✅ Stay on Bill Index
        }

        // ✅ Get Feedback for a Bill
        [HttpGet]
        public async Task<IActionResult> GetFeedbacks(int billId)
        {
            var feedbacks = await _context.UserFeedbacks
                .Where(f => f.BillId == billId)
                .Include(f => f.User)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new
                {
                    feedbackId = f.Id,
                    userId = f.UserId,
                    userFullName = f.User.FullName,
                    feedbackText = f.FeedbackText,
                    dateSubmitted = f.SubmittedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(feedbacks);
        }

        // ✅ Delete Feedback (Only Owner Can Delete)
        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var feedback = await _context.UserFeedbacks.FindAsync(feedbackId);
            if (feedback == null)
                return NotFound("Feedback not found.");

            if (feedback.UserId != userId)
                return Forbid(); // Only the owner can delete

            _context.UserFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return Ok("Feedback deleted successfully.");
        }


    }
}
