using Backend.Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Backend.Controllers
{
    [ApiController]
    [Route("pick")]
[Route("api/pick")]
    public class PickController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger<PickController> _logger;
        private readonly IPickService _pickService;
        private readonly IOrderService _orderService;

        public PickController(IPickService pickService, IOrderService orderService, Microsoft.Extensions.Logging.ILogger<PickController> logger)
        {
            _pickService = pickService;
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Pick([FromBody] PickRequest request)
        {
            try
            {
                await _pickService.ConfirmPick(request.OrderId, request.ProductBarcode, request.Rack, request.Slot);
                var updatedOrder = await _orderService.GetOrderResponseById(request.OrderId);
                return Ok(updatedOrder);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Pick request failed because the order was not found for {OrderId}", request.OrderId);
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Pick request failed validation for {OrderId}", request.OrderId);
                return BadRequest();
            }
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
