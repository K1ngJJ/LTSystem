using LTS_app.Models;
using System.Threading.Tasks;

namespace LTS_app.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> AuthenticateUserAsync(string username, string password);
        Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string password, string role);
        Task<(bool Success, string Message)> VerifyEmailAsync(string token);
    }
}
