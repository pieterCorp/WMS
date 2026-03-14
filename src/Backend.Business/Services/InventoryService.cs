using Backend.Data;
using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

using Backend.Data;
using Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Business.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly WarehouseDbContext _context;
        public InventoryService(WarehouseDbContext context)
        {
            _context = context;
        }
        public async Task<int> GetInventory(int productId, int locationId)
        {
            var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId);
            return inventory?.Quantity ?? 0;
        }
        public async Task<bool> ValidateRackSlot(int productId, int rack, int slot)
        {
            var inventory = await _context.Inventory.Include(i => i.Location).FirstOrDefaultAsync(i => i.ProductId == productId && i.Location != null && i.Location.Rack == rack && i.Location.Slot == slot);
            return inventory != null;
        }

        public async Task<bool> ReduceInventory(int productId, int locationId, int amount)
        {
            var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == productId && i.LocationId == locationId);
            if (inventory == null || inventory.Quantity < amount) return false;
            // Use transaction for atomicity
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                inventory.Quantity -= amount;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            return true;
        }
    }
}