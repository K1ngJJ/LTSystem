using LTS_app.Models;
using System.Threading.Tasks;

namespace LTS_app.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> AuthenticateUserAsync(string username, string password);
        Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string role, string fullName);  // Added fullName parameter
        Task<(bool Success, string Message)> VerifyEmailAsync(string token);
        Task<(bool Success, string Message)> RequestPasswordResetAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword);
    }
}
