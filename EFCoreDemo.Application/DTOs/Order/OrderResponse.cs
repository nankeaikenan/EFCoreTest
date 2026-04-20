namespace EFCoreDemo.Application.DTOs.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; } = new();
    }
}
