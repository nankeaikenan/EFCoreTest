using EFCoreDemo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Price).HasPrecision(18, 2);
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // Relationship with Category (One-to-Many)
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting category if it has products
        }
    }
}