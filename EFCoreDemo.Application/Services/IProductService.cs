using EFCoreDemo.Application.DTOs.Product;

namespace EFCoreDemo.Application.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponse>> GetProductsAsync();
        Task<ProductResponse?> GetProductByIdAsync(Guid id);
        Task<ProductResponse> AddProductAsync(CreateProductRequest request);
        Task UpdateProductAsync(Guid id, UpdateProductRequest request);
        Task DeleteProductAsync(Guid id);
        bool ProductExists(Guid id);
    }
}