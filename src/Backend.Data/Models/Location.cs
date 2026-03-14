namespace Backend.Data.Models;

public class Location
{
    public int Id { get; set; }
    public string? Aisle { get; set; }
    public int Rack { get; set; }
    public int Slot { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }
}
