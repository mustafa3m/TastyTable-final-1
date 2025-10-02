using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TastyTable.Core.Entities;
using TastyTable.Data;
using TastyTable.Services;
using Xunit;

namespace TastyTable.Tests
{
    public class UserServiceValidateTests
    {
        [Fact]
        public async Task ValidateUserAsync_ReturnsUser_WhenCredentialsAreValid()
        {
            // Arrange: InMemory DB with one user
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("users_validate_ok")
                .Options;

            using var ctx = new AppDbContext(options);
            var password = "P@ssw0rd!";
            var user = new User
            {
                Username = "u1",
                Email = "u1@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            // Use the real UserService; adjust if your constructor differs.
            var service = new UserService(ctx);

            // Act
            var validated = await service.ValidateUserAsync("u1", password);

            // Assert
            Assert.NotNull(validated);
            Assert.Equal("u1", validated!.Username);
            Assert.Equal("User", validated.Role);
        }

        [Fact]
        public async Task ValidateUserAsync_ReturnsNull_WhenPasswordIsWrong()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("users_validate_fail")
                .Options;

            using var ctx = new AppDbContext(options);
            var user = new User
            {
                Username = "u2",
                Email = "u2@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct$123"),
                Role = "User"
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var service = new UserService(ctx);

            var validated = await service.ValidateUserAsync("u2", "WrongPassword!");

            Assert.Null(validated);
        }
    }
}
