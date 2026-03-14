using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("inventory")]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly WarehouseDbContext _context;

        public InventoryController(WarehouseDbContext context)
        {
            _context = context;
        }

        [HttpGet("product/{productId}/location/{locationId}")]
        public async Task<IActionResult> GetInventory(int productId, int locationId)
        {
            var inv = await _context.Inventory
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId);

            if (inv == null)
                return NotFound();

            return Ok(new { Quantity = inv.Quantity });
        }
    }
}
