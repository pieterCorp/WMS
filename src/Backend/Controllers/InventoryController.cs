using Backend.Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Backend.Controllers
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
            try
            {
                var quantity = await _inventoryService.GetInventory(productId, locationId);
                return Ok(new { Quantity = quantity });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
