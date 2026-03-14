namespace Backend.Data.Models;

public class Inventory
{
    public int ProductId { get; set; }
    public int LocationId { get; set; }
    public int Quantity { get; set; }

    public Product? Product { get; set; }
    public Location? Location { get; set; }
}
