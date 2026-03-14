using System.Threading.Tasks;

namespace Backend.Business.Services
{
    public interface IPickService
    {
        /// <summary>
        /// Confirms a pick for an order using product barcode and rack/slot.
        /// Throws exceptions for error conditions.
        /// </summary>
        Task<bool> ConfirmPick(string orderId, string productBarcode, int rack, int slot);
    }
}
