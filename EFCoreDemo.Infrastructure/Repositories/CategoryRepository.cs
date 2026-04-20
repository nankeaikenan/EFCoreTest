using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Infrastructure.Data;

namespace EFCoreDemo.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        // 可以添加 Category 特有的仓储实现
    }
}