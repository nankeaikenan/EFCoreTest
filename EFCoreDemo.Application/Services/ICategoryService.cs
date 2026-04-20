using EFCoreDemo.Application.DTOs.Category;

namespace EFCoreDemo.Application.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id);
        Task<CategoryResponse> AddCategoryAsync(CreateCategoryRequest request);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(Guid id);
        bool CategoryExists(Guid id);
    }
}