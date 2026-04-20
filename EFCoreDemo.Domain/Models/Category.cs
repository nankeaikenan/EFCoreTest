using System;
using System.Collections.Generic;

namespace EFCoreDemo.Domain.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for related Products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}