using Microsoft.EntityFrameworkCore;
using TastyTable.Data;
using TastyTable.Services;
using TastyTable.Core.DTOs;
using Xunit;

namespace TastyTable.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_ComputesTotal()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "orders_test_db")
            .Options;

        using var ctx = new AppDbContext(options);
        ctx.MenuItems.Add(new TastyTable.Core.Entities.MenuItem { Id = 1, Name = "A", Price = 100, IsAvailable = true });
        ctx.MenuItems.Add(new TastyTable.Core.Entities.MenuItem { Id = 2, Name = "B", Price = 50, IsAvailable = true });
        await ctx.SaveChangesAsync();

        var svc = new OrderService(ctx);
        var req = new CreateOrderRequest(new List<OrderItemDto> { new(1,2), new(2,3) });
        var order = await svc.CreateAsync(5, req);

        Assert.Equal(2*100 + 3*50, order.Total);
        Assert.Equal(5, order.UserId);
    }
}