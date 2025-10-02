using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TastyTable.Core.DTOs;
using TastyTable.Core.Entities;
using TastyTable.Core.Interfaces;
using TastyTable.Data;

namespace TastyTable.Services
{
    public class OrderService : IOrderService
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "Pending", "Paid", "Cancelled", "Completed" };

        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        // Create a new order for a user
        public async Task<Order> CreateAsync(int userId, CreateOrderRequest request)
        {
            if (request?.Items is null || request.Items.Count == 0)
                throw new ArgumentException("Order must contain at least one item.", nameof(request));

            var menuIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();

            // Only allow available menu items
            var menus = await _context.MenuItems
                .Where(m => menuIds.Contains(m.Id) && m.IsAvailable)
                .ToDictionaryAsync(m => m.Id);

            // Validate
            foreach (var it in request.Items)
            {
                if (!menus.ContainsKey(it.MenuItemId))
                    throw new InvalidOperationException($"Menu item {it.MenuItemId} not found or unavailable.");
                if (it.Quantity <= 0)
                    throw new InvalidOperationException($"Quantity must be > 0 for item {it.MenuItemId}.");
            }

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            foreach (var it in request.Items)
            {
                var price = menus[it.MenuItemId].Price;
                order.Items.Add(new OrderItem
                {
                    MenuItemId = it.MenuItemId,
                    Quantity = it.Quantity,
                    UnitPrice = price
                });
            }

            order.Total = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        // List orders for a user
        public async Task<IEnumerable<Order>> GetForUserAsync(int userId)
        {
            return await _context.Orders.AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .ToListAsync();
        }

        // Get a specific order for a user
        public async Task<Order?> GetByIdAsync(int id, int userId)
        {
            return await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        }

        // Delete (Admin can delete any; user can delete own Pending)
        public async Task<bool> DeleteAsync(int id, int requesterUserId, bool isAdmin)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return false;

            if (!isAdmin)
            {
                if (order.UserId != requesterUserId) return false;
                if (!order.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)) return false;
            }

            if (order.Items?.Count > 0)
                _context.OrderItems.RemoveRange(order.Items);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        // Update items (Admin any; user own Pending)
        public async Task<Order?> UpdateItemsAsync(int id, int requesterUserId, bool isAdmin, UpdateOrderItemsRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return null;

            if (!isAdmin)
            {
                if (order.UserId != requesterUserId) return null;
                if (!order.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)) return null;
            }

            if (request?.Items is null || request.Items.Count == 0)
                throw new ArgumentException("Items are required.", nameof(request));

            var ids = request.Items.Select(i => i.MenuItemId).Distinct().ToList();

            var menus = await _context.MenuItems
                .Where(m => ids.Contains(m.Id) && m.IsAvailable)
                .ToDictionaryAsync(m => m.Id);

            foreach (var it in request.Items)
            {
                if (it.Quantity <= 0)
                    throw new InvalidOperationException($"Quantity must be > 0 for item {it.MenuItemId}.");
                if (!menus.ContainsKey(it.MenuItemId))
                    throw new InvalidOperationException($"Menu item {it.MenuItemId} not found or unavailable.");
            }

            // Replace items
            if (order.Items?.Count > 0)
                _context.OrderItems.RemoveRange(order.Items);

            order.Items = request.Items.Select(it => new OrderItem
            {
                MenuItemId = it.MenuItemId,
                Quantity = it.Quantity,
                UnitPrice = menus[it.MenuItemId].Price
            }).ToList();

            order.Total = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            await _context.SaveChangesAsync();
            return order;
        }

        // Admin-only status change
        public async Task<Order?> UpdateStatusAsync(int id, string status)
        {
            if (!AllowedStatuses.Contains(status))
                throw new ArgumentException(
                    $"Status '{status}' is not allowed. Allowed: {string.Join(", ", AllowedStatuses)}",
                    nameof(status));

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return null;

            order.Status = status;
            await _context.SaveChangesAsync();
            return order;
        }
    }
}
