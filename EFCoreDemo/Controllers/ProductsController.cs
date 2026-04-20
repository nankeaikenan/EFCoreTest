using Microsoft.AspNetCore.Mvc;
using EFCoreDemo.Application.Services;
using EFCoreDemo.Application.DTOs.Product;

namespace EFCoreDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>> GetProduct(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> PostProduct(CreateProductRequest request)
        {
            var productResponse = await _productService.AddProductAsync(request);

            return CreatedAtAction(nameof(GetProduct), new { id = productResponse.Id }, productResponse);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, UpdateProductRequest request)
        {
            if (!_productService.ProductExists(id))
            {
                return NotFound();
            }

            await _productService.UpdateProductAsync(id, request);

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            if (!_productService.ProductExists(id))
            {
                return NotFound();
            }

            await _productService.DeleteProductAsync(id);

            return NoContent();
        }
    }
}