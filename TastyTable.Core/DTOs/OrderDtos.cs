using System.Collections.Generic;

namespace TastyTable.Core.DTOs
{

    public record OrderItemDto(int MenuItemId, int Quantity);
    public record CreateOrderRequest(List<OrderItemDto> Items);

    public record OrderReadDto(int Id, DateTime CreatedAt, decimal Total, string Status);
    public record OrderItemReadDto(int Id, int MenuItemId, string MenuItemName, int Quantity, decimal UnitPrice);
    public record OrderWithItemsReadDto(int Id, DateTime CreatedAt, decimal Total, string Status, List<OrderItemReadDto> Items);
    public record UpdateOrderItemsRequest(List<UpdateOrderItemDto> Items);

    public record UpdateOrderItemDto(int MenuItemId, int Quantity);
    public record UpdateOrderStatusRequest(string Status);
}
