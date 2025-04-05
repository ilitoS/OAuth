using Microsoft.EntityFrameworkCore;
using OAuth.Server.Models;
using System.Security.Cryptography;
using System.Text;

namespace OAuth.Server.Services
{
    public class UserService : IUserService
    {
        private readonly OauthAppContext _context;
        public UserService(OauthAppContext oauthAppContext)
        {
            _context = oauthAppContext;
        }

        public async Task<User> SaveUserAsync(string email, string username, string accessToken,  string refreshToken, string authType, DateTime? expiryDate, string? password = null)
        {
            var userEntity = new User
            {
                Username = username,
                AccessToken = accessToken,
                Email = email,
                RefreshToken = refreshToken,
                AuthType = authType,
                ExpireTimestamp = expiryDate ?? DateTime.UtcNow.AddDays(7),
                Password = null,
            };

            if (password != null )
                userEntity.Password = HashPassword(password);

            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();

            return userEntity;
        }
        
        public User? GetUserByEmail(string email, string authType)
        {
            return _context.Users.FirstOrDefault(x => x.Email == email && x.AuthType == authType);
        }
        public User? GetUserByUsername(string username, string authType)
        {
            return _context.Users.FirstOrDefault(x => x.Username == username && x.AuthType == authType);
        }

        public bool IsPasswordValid(User user, string password)
        {
            return HashPassword(password) == user.Password;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
