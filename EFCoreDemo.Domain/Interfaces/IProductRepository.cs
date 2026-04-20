using EFCoreDemo.Domain.Models;

namespace EFCoreDemo.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<Product?> GetProductWithCategoryAsync(Guid id);
    }
}