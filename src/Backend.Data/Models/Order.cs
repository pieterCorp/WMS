namespace Backend.Data.Models;

public class Order
{
    public int Id { get; set; }
    public string? OrderId { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
