using Backend.Data;
using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Business.Services
{
    public class PickService : IPickService
    {
        private readonly WarehouseDbContext _context;
        public PickService(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ConfirmPick(string orderId, string productBarcode, int rack, int slot)
        {
            var order = await _context.Orders.Include(o => o.Items).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");
            var orderItem = order.Items.FirstOrDefault(oi => oi.Product != null && oi.Product.Barcode == productBarcode);
            if (orderItem == null) throw new InvalidOperationException("Product barcode does not match order item");
            var inventory = await _context.Inventory.Include(i => i.Location).FirstOrDefaultAsync(i => i.ProductId == orderItem.ProductId && i.Location != null && i.Location.Rack == rack && i.Location.Slot == slot);
            if (inventory == null) throw new InvalidOperationException("Rack/slot does not match expected location");
            if (inventory.Quantity <= 0) throw new InvalidOperationException("No inventory at location");
            inventory.Quantity -= 1;
            orderItem.PickedQuantity += 1;
            if (order.Items.All(oi => oi.PickedQuantity >= oi.Quantity)) order.Status = "Picked";
            var scanEvent = new ScanEvent { ProductId = orderItem.ProductId, Rack = rack.ToString(), Slot = slot.ToString(), Timestamp = DateTime.UtcNow, Action = "Pick" };
            _context.ScanEvents.Add(scanEvent);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}