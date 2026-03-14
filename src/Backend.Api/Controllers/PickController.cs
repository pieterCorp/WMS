using Backend.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("pick")]
    [Route("api/pick")]
    public class PickController : ControllerBase
    {
        private readonly ILogger<PickController> _logger;
        private readonly IPickService _pickService;
        private readonly IOrderService _orderService;

        public PickController(IPickService pickService, IOrderService orderService, ILogger<PickController> logger)
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
                var ok = await _pickService.ConfirmPick(request.OrderId, request.ProductBarcode, request.Rack, request.Slot);
                if (!ok) return BadRequest();

                var orderDto = await _orderService.GetOrderResponseById(request.OrderId);
                if (orderDto == null) return NotFound();
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pick failed");
                return BadRequest(ex.Message);
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
