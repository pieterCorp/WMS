using Backend.Data;
using Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Business.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("orders")]
[Route("api/orders")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(WarehouseDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId)
    {
        var orders = await _context.Orders.ToListAsync();
        _logger.LogInformation($"Order count: {orders.Count}, OrderIds: [{string.Join(",", orders.Select(o => o.OrderId))}]");

        var order = orders
            .Where(o => o.OrderId == orderId)
            .Select(o => _context.Orders
                .Include(x => x.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(x => x.OrderId == o.OrderId))
            .FirstOrDefault();

        if (order == null)
        {
            _logger.LogWarning($"Order not found for orderId: {orderId}");
            return NotFound();
        }

        var response = new OrderResponse
        {
            OrderId = order.OrderId,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                Product = item.Product?.Name ?? "Unknown",
                Sku = item.Product?.Sku ?? "Unknown",
                Location = GetLocationFromOrderItem(item),
                Quantity = item.Quantity
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
