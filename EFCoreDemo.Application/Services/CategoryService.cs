using EFCoreDemo.Application.DTOs.Category;
using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;

using AutoMapper;

namespace EFCoreDemo.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> AddCategoryAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow  // 业务逻辑在 Service 层处理
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return;

            category.Name = request.Name;
            category.Description = request.Description;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                _categoryRepository.Remove(category);
                await _categoryRepository.SaveChangesAsync();
            }
        }

        public bool CategoryExists(Guid id)
        {
            return _categoryRepository.FindAsync(c => c.Id == id).Result.Any();
        }
    }
}