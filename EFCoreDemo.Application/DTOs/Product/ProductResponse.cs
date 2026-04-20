namespace EFCoreDemo.Application.DTOs.Product
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }  // 扁平化导航属性，避免循环引用
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
