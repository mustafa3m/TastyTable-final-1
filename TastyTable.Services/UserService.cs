using Microsoft.EntityFrameworkCore;
using TastyTable.Core.Entities;
using TastyTable.Core.Interfaces;
using TastyTable.Data;

namespace TastyTable.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _ctx;

        public UserService(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null) return null;

            var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return ok ? user : null;
        }

        public Task<User?> GetByUsernameAsync(string username)
            => _ctx.Users.FirstOrDefaultAsync(u => u.Username == username)!;

        public async Task<User> RegisterAsync(string username, string password, string email)
        {
            // Uniqueness checks
            var exists = await _ctx.Users.AnyAsync(u => u.Username == username || u.Email == email);
            if (exists) throw new InvalidOperationException("Username or email already exists.");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };

            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
            return user;
        }

        public Task<User?> GetByIdAsync(int id)
            => _ctx.Users.FindAsync(id).AsTask();
    }
}
