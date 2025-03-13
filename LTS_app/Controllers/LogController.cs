using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LTS_app.Controllers
{
    [Authorize(Roles = "Admin")] // 🔹 Only Admins can view logs
    public class LogController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: UserLogs
        public async Task<IActionResult> Index()
        {
            var logs = await _context.UserLogs.OrderByDescending(l => l.Timestamp).ToListAsync();
            return View(logs);
        }
    }
}
