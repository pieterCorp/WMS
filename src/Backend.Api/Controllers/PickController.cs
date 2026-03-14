using Backend.Data;
using Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("pick")]
    [Route("api/pick")]
    public class PickController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger<PickController> _logger;
        private readonly WarehouseDbContext _context;

        public PickController(WarehouseDbContext context, Microsoft.Extensions.Logging.ILogger<PickController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Pick([FromBody] PickRequest request)
        {
            // 1. Validate order
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);
            if (order == null)
                return NotFound();

            // 2. Find order item by barcode
            var orderItem = order.Items.FirstOrDefault(oi => oi.Product != null && oi.Product.Barcode == request.ProductBarcode);
            if (orderItem == null)
                return BadRequest();

            // 3. Validate rack/slot
            var inventory = await _context.Inventory
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.ProductId == orderItem.ProductId && i.Location != null && i.Location.Rack == request.Rack && i.Location.Slot == request.Slot);
            if (inventory == null)
                return BadRequest();

            // 4. Reduce inventory
            _logger.LogInformation("Inventory before decrement: ProductId={ProductId}, LocationId={LocationId}, Quantity={Quantity}", inventory.ProductId, inventory.LocationId, inventory.Quantity);
            if (inventory.Quantity <= 0)
                return BadRequest();
            inventory.Quantity -= 1;
            _logger.LogInformation("Inventory after decrement: ProductId={ProductId}, LocationId={LocationId}, Quantity={Quantity}", inventory.ProductId, inventory.LocationId, inventory.Quantity);

            // 5. Increase picked quantity
            orderItem.PickedQuantity += 1;

            // 6. Update order status if all items picked
            if (order.Items.All(oi => oi.PickedQuantity >= oi.Quantity))
                order.Status = "Picked";

            // 7. Log scan event
            var scanEvent = new ScanEvent
            {
                ProductId = orderItem.ProductId,
                Rack = request.Rack.ToString(),
                Slot = request.Slot.ToString(),
                Timestamp = DateTime.UtcNow,
                Action = "Pick"
            };
            _context.ScanEvents.Add(scanEvent);

            await _context.SaveChangesAsync();

            // 8. Return updated order progress
            var response = new
            {
                orderId = order.OrderId,
                status = order.Status,
                items = order.Items.Select(item => new
                {
                    product = item.Product?.Name ?? "Unknown",
                    sku = item.Product?.Sku ?? "Unknown",
                    location = item.Product != null && inventory.Location != null ? $"{inventory.Location.Rack}-{inventory.Location.Slot}" : "Unknown",
                    quantity = item.Quantity,
                    pickedQuantity = item.PickedQuantity
                }).ToList()
            };
            return Ok(response);
        }
    }

    public class PickRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string ProductBarcode { get; set; } = string.Empty;
        public int Rack { get; set; }
        public int Slot { get; set; }
    }
}
