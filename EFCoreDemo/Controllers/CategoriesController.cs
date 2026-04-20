using Microsoft.AspNetCore.Mvc;
using EFCoreDemo.Application.Services;
using EFCoreDemo.Application.DTOs.Category;

namespace EFCoreDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetCategory(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> PostCategory(CreateCategoryRequest request)
        {
            var categoryResponse = await _categoryService.AddCategoryAsync(request);

            return CreatedAtAction(nameof(GetCategory), new { id = categoryResponse.Id }, categoryResponse);
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(Guid id, UpdateCategoryRequest request)
        {
            if (!_categoryService.CategoryExists(id))
            {
                return NotFound();
            }

            await _categoryService.UpdateCategoryAsync(id, request);

            return NoContent();
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            if (!_categoryService.CategoryExists(id))
            {
                return NotFound();
            }

            await _categoryService.DeleteCategoryAsync(id);

            return NoContent();
        }
    }
}