using System;
using System.Collections.Generic;

namespace EFCoreDemo.Domain.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = string.Empty; // 订单编号
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ShippingAddress { get; set; }

        // Navigation property for OrderDetails
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}