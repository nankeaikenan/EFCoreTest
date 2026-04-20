using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoryAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<Product?> GetProductWithCategoryAsync(Guid id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}