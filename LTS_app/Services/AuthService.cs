using LTS_app.Data;
using LTS_app.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace LTS_app.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        public async Task<(bool Success, string Message, User? User)> AuthenticateUserAsync(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);

            if (user == null)
                return (false, "Invalid username or password.", null);

            if (!user.IsConfirmed)
                return (false, "Please verify your email before logging in.", null);

            if (!user.IsActive)
                return (false, "Your account is disabled. Contact admin.", null);

            return (true, "Login successful.", user);
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string role, string fullName)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
                return (false, "All fields are required.");

            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                return (false, "Username or Email is already taken.");

            var validRoles = new[] { "Admin", "Legislator", "User" };
            if (!validRoles.Contains(role))
                return (false, "Invalid role.");

            string hashedPassword = HashPassword(password);
            string confirmationToken = GenerateToken();

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                Role = await _context.Users.AnyAsync() ? role : "Admin",  // Default to "Admin" for the first user
                Token = GenerateToken(),
                IsActive = true, // Users are active by default
                IsConfirmed = false, // Must verify email
                ConfirmationToken = confirmationToken,
                FullName = fullName, // Store the FullName
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await SendVerificationEmailAsync(email, confirmationToken);

            return (true, "Registration successful! Please check your email to verify your account.");
        }


        public async Task<(bool Success, string Message)> VerifyEmailAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return (false, "Invalid verification token.");

            string decodedToken = WebUtility.UrlDecode(token);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ConfirmationToken == decodedToken);
            if (user == null)
                return (false, "Invalid verification token.");

            if (user.IsConfirmed)
                return (true, "Your account is already verified.");

            user.IsConfirmed = true;
            user.ConfirmationToken = null;
            await _context.SaveChangesAsync();

            return (true, "Your email has been verified successfully! You can now log in.");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return string.Concat(bytes.Select(b => b.ToString("x2")));
            }
        }

        private string GenerateToken()
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(tokenBytes);
        }

        private async Task SendVerificationEmailAsync(string email, string token)
        {
            var smtpUser = _config["EmailSettings:SmtpUser"];
            var smtpPassword = _config["EmailSettings:SmtpPassword"];
            var smtpHost = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);

            var confirmationLink = $"https://{_config["AppDomain"]}/Auth/VerifyEmail?token={WebUtility.UrlEncode(token)}";
            var fromAddress = new MailAddress(smtpUser, "LTS App");
            var toAddress = new MailAddress(email);
            const string subject = "Email Verification - LTS App";
            string body = $"Click the link to verify your account: <a href='{confirmationLink}'>Verify Email</a>";

            using (var smtp = new SmtpClient(smtpHost, smtpPort) { EnableSsl = true, Credentials = new NetworkCredential(smtpUser, smtpPassword) })
            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body, IsBodyHtml = true })
            {
                await smtp.SendMailAsync(message);
            }
        }
    }
}
