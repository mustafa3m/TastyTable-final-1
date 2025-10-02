using TastyTable.Core.DTOs;
using TastyTable.Core.Entities;

namespace TastyTable.Core.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(int userId, CreateOrderRequest request);
    Task<IEnumerable<Order>> GetForUserAsync(int userId);
    Task<Order?> GetByIdAsync(int id, int userId);
    Task<bool> DeleteAsync(int id, int requesterUserId, bool isAdmin);
    Task<Order?> UpdateItemsAsync(int id, int requesterUserId, bool isAdmin, UpdateOrderItemsRequest request);
    Task<Order?> UpdateStatusAsync(int id, string status);
}