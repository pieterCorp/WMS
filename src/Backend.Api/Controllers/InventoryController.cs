using Backend.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("inventory")]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("product/{productId}/location/{locationId}")]
        public async Task<IActionResult> GetInventory(int productId, int locationId)
        {
            var qty = await _inventoryService.GetInventory(productId, locationId);
            if (qty == 0) return NotFound();
            return Ok(new { Quantity = qty });
        }
    }
}
