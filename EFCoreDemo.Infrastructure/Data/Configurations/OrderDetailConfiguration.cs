using EFCoreDemo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Infrastructure.Data.Configurations
{
    public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
            builder.Property(e => e.Subtotal).HasPrecision(18, 2);

            // Relationship with Order (One-to-Many)
            builder.HasOne(d => d.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // If order is deleted, delete details

            // Relationship with Product (One-to-Many)
            builder.HasOne(d => d.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting product if it's in an order
        }
    }
}