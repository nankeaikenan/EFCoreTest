namespace EFCoreDemo.Domain.Models
{
    public class OrderDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }      // 外键到 Order
        public Guid ProductId { get; set; }    // 外键到 Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  // 下单时的单价
        public decimal Subtotal { get; set; }  // 小计：Quantity * UnitPrice

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        public Order? Order { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public Product? Product { get; set; }
    }
}