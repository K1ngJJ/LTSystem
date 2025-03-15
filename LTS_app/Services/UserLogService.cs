using System.Threading.Tasks;
using LTS_app.Data;
using LTS_app.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LTS_app.Services
{
    public class UserLogService : IUserLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActivityAsync(int userId, string username, string fullName, string action, string ipAddress)
        {
            var log = new UserLog
            {
                UserId = userId,
                Username = username,
                FullName = fullName, // 🔹 Include Full Name
                Action = action,
                IPAddress = ipAddress,
                Timestamp = DateTime.Now
            };

            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
