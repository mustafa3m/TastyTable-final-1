using System.Text.Json.Serialization;

namespace TastyTable.Core.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [JsonIgnore] // prevent Order -> Items -> Order recursion
    public Order? Order { get; set; }

    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}
