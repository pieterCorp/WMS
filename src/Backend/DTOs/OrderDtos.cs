namespace Backend.DTOs
{
    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    }

    public class OrderItemResponse
    {
        public string Product { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
