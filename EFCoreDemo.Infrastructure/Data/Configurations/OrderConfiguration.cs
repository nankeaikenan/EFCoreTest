using EFCoreDemo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        }
    }
}