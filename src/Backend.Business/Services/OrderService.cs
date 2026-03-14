using Backend.Data;
using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Business.Services
{
    public class OrderService
    {
        private readonly WarehouseDbContext _context;
        public OrderService(WarehouseDbContext context)
        {
            _context = context;
        }
        public async Task<Order?> GetOrderById(string orderId)
        {
            return await _context.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
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