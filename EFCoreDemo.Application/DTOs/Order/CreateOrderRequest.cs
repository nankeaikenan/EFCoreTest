namespace EFCoreDemo.Application.DTOs.Order
{
    public class CreateOrderRequest
    {
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ShippingAddress { get; set; }
        public List<CreateOrderDetailRequest> OrderDetails { get; set; } = new();
    }
}
