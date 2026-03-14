using System.Threading.Tasks;

namespace Backend.Business.Services
{
    public interface IInventoryService
    {
        Task<int> GetInventory(int productId, int locationId);
        Task<bool> ValidateRackSlot(int productId, int rack, int slot);
        Task<bool> ReduceInventory(int productId, int locationId, int amount);
    }
}
