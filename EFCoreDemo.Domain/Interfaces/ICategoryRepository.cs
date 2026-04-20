using EFCoreDemo.Domain.Models;

namespace EFCoreDemo.Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // 可以添加 Category 特有的方法
    }
}