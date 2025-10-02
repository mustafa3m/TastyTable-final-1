using TastyTable.Core.Entities;

namespace TastyTable.Core.Interfaces
{
    public interface IUserService
    {
        /// <summary>Returns the user if username/password are valid; otherwise null.</summary>
        Task<User?> ValidateUserAsync(string username, string password);

        /// <summary>Get a user by username, or null if not found.</summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>Create a new user (throws if username/email already exists).</summary>
        Task<User> RegisterAsync(string username, string password, string email);

        /// <summary>Get by id, or null if not found.</summary>
        Task<User?> GetByIdAsync(int id);
    }
}
