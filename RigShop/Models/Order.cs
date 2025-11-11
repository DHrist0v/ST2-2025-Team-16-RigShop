using System.ComponentModel.DataAnnotations;

namespace RigShop.Web.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // relationships
    public int UserId { get; set; }
    public User? User { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    // All
    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);
}