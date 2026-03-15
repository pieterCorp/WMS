using Backend.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Business.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("orders")]
[Route("api/orders")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId)
    {
        var order = await _orderService.GetOrderResponseById(orderId);

        if (order == null)
        {
            _logger.LogWarning($"Order not found for orderId: {orderId}");
            return NotFound();
        }

        return Ok(order);
    }
}
