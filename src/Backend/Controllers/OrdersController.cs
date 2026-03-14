using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("orders")]
[Route("api/orders")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly WarehouseDbContext _context;

    public OrdersController(WarehouseDbContext context)
    {
        _context = context;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return NotFound();

        var response = new
        {
            orderId = order.OrderId,
            items = order.Items.Select(item => new
            {
                product = item.Product?.Name ?? "Unknown",
                sku = item.Product?.Sku ?? "Unknown",
                location = GetLocationFromOrderItem(item),
                quantity = item.Quantity
            }).ToList()
        };

        return Ok(response);
    }

    private string GetLocationFromOrderItem(OrderItem item)
    {
        var inventory = _context.Inventory
            .Include(i => i.Location)
            .FirstOrDefault(i => i.ProductId == item.ProductId);

        if (inventory?.Location == null)
            return "Unknown";

        var location = inventory.Location;
        return $"{location.Aisle}{location.Rack}-{location.Slot}";
    }
}
