namespace EFCoreDemo.Application.DTOs.Order
{
    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }  // 扁平化导航属性
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
