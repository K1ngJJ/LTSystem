using System.Threading.Tasks;

namespace LTS_app.Services
{
    public interface IUserLogService
    {
        Task LogActivityAsync(int userId, string username, string fullName, string action, string ipAddress);
    }
}
