using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TastyTable.Core.DTOs;
using TastyTable.Core.Interfaces;

namespace TastyTable.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    public OrdersController(IOrderService orders) => _orders = orders;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                  User.FindFirst("sub")?.Value ?? "0");

    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var userId = GetUserId();
        var order = await _orders.CreateAsync(userId, request);
        return Ok(new OrderReadDto(order.Id, order.CreatedAt, order.Total, order.Status));
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var userId = GetUserId();
        var orders = await _orders.GetForUserAsync(userId);
        var list = orders.Select(o => new OrderReadDto(o.Id, o.CreatedAt, o.Total, o.Status));
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var o = await _orders.GetByIdAsync(id, userId);
        if (o is null) return NotFound();

        var dto = new OrderWithItemsReadDto(
            o.Id,
            o.CreatedAt,
            o.Total,
            o.Status,
            o.Items.Select(i => new OrderItemReadDto(
                i.Id,
                i.MenuItemId,
                i.MenuItem?.Name ?? string.Empty,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItems(int id, UpdateOrderItemsRequest request)
    {
        var updated = await _orders.UpdateItemsAsync(id, GetUserId(), IsAdmin(), request);
        if (updated is null) return Forbid();

        return Ok(new
        {
            updated.Id,
            updated.Total,
            updated.Status,
            Items = updated.Items.Select(i => new
            {
                i.Id, i.MenuItemId, i.Quantity, i.UnitPrice
            })
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest req)
    {
        var updated = await _orders.UpdateStatusAsync(id, req.Status);
        return updated is null ? NotFound() : Ok(new { message = "Status updated", updated.Id, updated.Status });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var ok = await _orders.DeleteAsync(id, GetUserId(), IsAdmin());
        if (!ok) return Forbid(); // or NotFound()
        return Ok(new { message = "Order deleted successfully" });
    }
}
