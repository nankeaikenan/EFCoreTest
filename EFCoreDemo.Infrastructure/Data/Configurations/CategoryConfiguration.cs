using EFCoreDemo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        }
    }
}