using Backend.Data;
using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

using Backend.Data;
using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Backend.Business.DTOs;

namespace Backend.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly WarehouseDbContext _context;
        public OrderService(WarehouseDbContext context)
        {
            _context = context;
        }

        // Return DTO mapped from entities
        public async Task<OrderResponse?> GetOrderResponseById(string orderId)
        {
            var order = await _context.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;
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
            return response;
        }

        // Backwards-compatible method for tests that call GetOrderById
        public async Task<Order?> GetOrderById(string orderId)
        {
            return await _context.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        private string GetLocationFromOrderItem(OrderItem item)
        {
            var inventory = _context.Inventory.Include(i => i.Location).FirstOrDefault(i => i.ProductId == item.ProductId);
            if (inventory?.Location == null) return "Unknown";
            var location = inventory.Location;
            return $"{location.Aisle}{location.Rack}-{location.Slot}";
        }

        public async Task<bool> UpdatePickedQuantity(string orderId, int productId, int pickedAmount)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return false;
            var item = order.Items.FirstOrDefault(oi => oi.ProductId == productId);
            if (item == null) return false;
            item.PickedQuantity += pickedAmount;
            if (order.Items.All(oi => oi.PickedQuantity >= oi.Quantity)) order.Status = "Picked";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}