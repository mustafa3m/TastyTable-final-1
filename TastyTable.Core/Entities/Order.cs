namespace TastyTable.Core.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled
}