using EFCoreDemo.Application.DTOs.Product;
using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using AutoMapper;

namespace EFCoreDemo.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsAsync()
        {
            var products = await _productRepository.GetProductsWithCategoryAsync();
            return _mapper.Map<IEnumerable<ProductResponse>>(products);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductWithCategoryAsync(id);
            return product == null ? null : _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse> AddProductAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow  // 业务逻辑在 Service 层处理
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            return _mapper.Map<ProductResponse>(product);
        }

        public async Task UpdateProductAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return;

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;  // 业务逻辑在 Service 层处理

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                _productRepository.Remove(product);
                await _productRepository.SaveChangesAsync();
            }
        }

        public bool ProductExists(Guid id)
        {
            return _productRepository.FindAsync(p => p.Id == id).Result.Any();
        }
    }
}