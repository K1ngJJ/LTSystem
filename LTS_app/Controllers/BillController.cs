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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BillController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's ID safely
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return Unauthorized(); // Return unauthorized if user ID is invalid
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value; // Get user role
            var user = await _context.Users.Include(u => u.Legislator).FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                ViewBag.UserName = user.FullName;
            }

            // Get all bills with related data
            IQueryable<Bill> billsQuery = _context.Bills
                .Include(b => b.User)
                .Include(b => b.Committee)
                .Include(b => b.Session);

            if (userRole == "Legislator" && user.Legislator != null)
            {
                int legislatorId = user.Legislator.Id; // Get LegislatorId of the logged-in user

                // Get committee IDs assigned to the legislator
                var userCommitteeIds = await _context.CommitteeLegislators
                    .Where(cl => cl.LegislatorId == legislatorId)
                    .Select(cl => cl.CommitteeId)
                    .ToListAsync();

                billsQuery = billsQuery.Where(b => b.CommitteeId != null && userCommitteeIds.Contains(b.CommitteeId.Value));
            }

            // Execute the query
            var bills = await billsQuery.ToListAsync();

            // Load committees and sessions for dropdowns
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




        [Authorize(Roles = "Admin,Legislator,User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     string Title,
     string Description,
     int CommitteeId,
     int SessionId,
     string Status,
     IFormFile? BillFile,
     IFormFile? ImageFile)  // ✅ Add file parameter
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (string.IsNullOrEmpty(Title) || CommitteeId == 0 || SessionId == 0)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Index));
            }

            string? filePath = null;
            string? imagePath = null;

            if (BillFile != null && BillFile.Length > 0)
            {
                // ✅ Ensure upload folder exists
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/documents");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // ✅ Save file with unique name
                string fileName = $"{Guid.NewGuid()}_{BillFile.FileName}";
                filePath = Path.Combine("/uploads/documents", fileName);  // Relative path for web access

                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await BillFile.CopyToAsync(stream);
                }
            }

            // ✅ Handle Image File Upload
            if (BillFile != null && BillFile.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/documents");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string fileName = $"{Guid.NewGuid()}_{BillFile.FileName}";
                filePath = Path.Combine("/uploads/documents", fileName);

                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await BillFile.CopyToAsync(stream);
                }
            }


            if (ImageFile != null && ImageFile.Length > 0)
            {
                var imagePathRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/images");
                if (!Directory.Exists(imagePathRoot)) Directory.CreateDirectory(imagePathRoot);

                // ✅ Ensure only image files are uploaded
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExt = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExt))
                {
                    TempData["ErrorMessage"] = "Invalid image format. Allowed formats: JPG, PNG, GIF, BMP, WEBP.";
                    return RedirectToAction(nameof(Index));
                }

                string imageFileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                imagePath = Path.Combine("/uploads/images", imageFileName);

                using (var stream = new FileStream(Path.Combine(imagePathRoot, imageFileName), FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
            }


            var createBill = new Bill
            {
                UserId = userId,
                Title = Title,
                Description = Description,
                CommitteeId = CommitteeId,
                SessionId = SessionId,
                Status = Enum.TryParse<BillStatus>(Status, out var billStatus) ? billStatus : BillStatus.Draft,
                IntroducedDate = DateTime.Now,
                FilePath = filePath,  // ✅ Save file path in DB
                ImagePath = imagePath // ✅ Save image path in DB
            };

            _context.Bills.Add(createBill);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Legislator,User")]
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


        [Authorize(Roles = "Admin,Legislator,User")]
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


        [HttpGet]
        public async Task<IActionResult> GetDropdownOldData()
        {
            var committees = await _context.Committees
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();

            var sessions = await _context.Sessions
                .Select(s => new { id = s.Id, name = s.Name })
                .ToListAsync();

            return Json(new { committees, sessions });
        }


        [HttpGet]
        public IActionResult GetBillDetails(int id)
        {
            var bill = _context.Bills
                .Include(b => b.User)
                .Include(b => b.Committee)
                .Include(b => b.Session)
                .FirstOrDefault(b => b.Id == id);

            if (bill == null)
                return NotFound();

            var userIsLegislator = User.IsInRole("Legislator"); // Check if user is a legislator

            return Json(new
            {
                id = bill.Id,
                title = bill.Title,
                description = bill.Description,
                status = bill.Status,
                proposedBy = bill.User?.FullName ?? "Unknown",
                committee = bill.Committee?.Name ?? "N/A",
                session = bill.Session?.Name ?? "N/A",
                introducedDate = bill.IntroducedDate,
                committeeId = bill.CommitteeId,
                sessionId = bill.SessionId,
                isLegislator = userIsLegislator
            });
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, int status)
        {
            var bill = _context.Bills.Find(id);
            if (bill == null)
            {
                return Json(new { success = false, message = "Bill not found." });
            }

            try
            {
                // Convert integer to Enum
                if (Enum.IsDefined(typeof(BillStatus), status))
                {
                    bill.Status = (BillStatus)status;
                    _context.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid status value." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



    }
}







