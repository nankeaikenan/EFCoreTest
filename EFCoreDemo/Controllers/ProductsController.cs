using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EFCoreDemo.Application.Services;
using EFCoreDemo.Application.DTOs.Product;

namespace EFCoreDemo.Controllers
{
    /// <summary>
    /// 商品管理接口。
    ///
    /// 【权限设计示例 - 粒度控制】
    ///   GET（查询）：公开访问，任何人可以浏览商品列表
    ///   POST/PUT/DELETE（写操作）：仅 Admin 角色可操作
    ///
    /// [Authorize] 属性说明：
    ///   - 类级别 [Authorize]：该控制器所有接口默认需要认证
    ///   - 方法级别 [AllowAnonymous]：覆盖类级别，允许匿名访问
    ///   - 方法级别 [Authorize(Roles = "Admin")]：覆盖类级别，追加角色要求
    ///
    /// 认证失败（未携带 Token）→ 401 Unauthorized
    /// 授权失败（Token 有效但角色不符）→ 403 Forbidden
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 查询接口（公开）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>获取全部商品（公开，无需登录）</summary>
        [HttpGet]
        [AllowAnonymous]  // 商品列表公开展示
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
        {
            var products = await _productService.GetProductsAsync();
            return Ok(products);
        }

        /// <summary>根据 ID 获取单个商品（公开，无需登录）</summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponse>> GetProduct(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 写入接口（仅 Admin）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 创建商品（仅 Admin 角色）。
        ///
        /// [Authorize(Roles = "Admin")]：
        ///   框架在执行 Action 前，先调用 JWT Bearer 中间件验证 Token，
        ///   再检查 Token Claims 中是否包含 Role = "Admin" 的声明。
        ///   不满足时自动返回 403 Forbidden（不会进入 Action 方法体）。
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductResponse>> PostProduct(CreateProductRequest request)
        {
            var productResponse = await _productService.AddProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = productResponse.Id }, productResponse);
        }

        /// <summary>更新商品（仅 Admin 角色）</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(Guid id, UpdateProductRequest request)
        {
            if (!_productService.ProductExists(id)) return NotFound();
            await _productService.UpdateProductAsync(id, request);
            return NoContent();
        }

        /// <summary>删除商品（仅 Admin 角色）</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            if (!_productService.ProductExists(id)) return NotFound();
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}