namespace EFCoreDemo.Application.DTOs.Order
{
    public class CreateOrderDetailRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        // UnitPrice 和 Subtotal 由 Service 层根据当前商品价格计算，客户端不需填写
    }
}
