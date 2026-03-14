using Backend.Business.DTOs;
using System.Threading.Tasks;

namespace Backend.Business.Services
{
    public interface IOrderService
    {
        Task<OrderResponse?> GetOrderResponseById(string orderId);
        Task<bool> UpdatePickedQuantity(string orderId, int productId, int pickedAmount);
    }
}
