namespace EFCoreDemo.Domain.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key for Category
        public Guid CategoryId { get; set; }

        // Navigation properties
        [System.Text.Json.Serialization.JsonIgnore]
        public Category? Category { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}