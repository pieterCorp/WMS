namespace Backend.Data.Models;

public class ScanEvent
{
    public int Id { get; set; }
    public string? Worker { get; set; }
    public int ProductId { get; set; }
    public string? Rack { get; set; }
    public string? Slot { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Action { get; set; }
}
