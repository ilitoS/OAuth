
using OAuth.Server.Models;

namespace OAuth.Server.Services
{
    public interface IUserService
    {
        Task<User> SaveUserAsync(string email, string username, string accessToken, string refreshToken, string authType, DateTime? expiryDate, string? password = null);
        bool IsPasswordValid(User user, string password);
        User? GetUserByEmail(string email, string authType);
        User? GetUserByUsername(string username, string authType);
    }
}
