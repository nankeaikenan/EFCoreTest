using EFCoreDemo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Infrastructure.Data.Configurations
{
    // 对一对多关系 Author & Book 的 Fluent API 映射配置
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            // 配置一对多映射关系
            builder
                // 当前实体 (Author) 有 "很多" (>0) 书籍关联属性 (Books)，这一步定义了 "多" (Many) 的那一头
                .HasMany(a => a.Books)
                // 反过来，任何一本书只属于 "一位" 或最多 "一位" 作者 (Author)，这一步定义了 "一" (One) 的那一头
                .WithOne(b => b.Author)
                // 指定存放外键的那个表的字段：Book 类里的 AuthorId 就是指向这个表的外键
                .HasForeignKey(b => b.AuthorId)
                // 级联删除选项（可选）：如果作者被删除了，他写的书会自动一起从数据库被删除
                .OnDelete(DeleteBehavior.Cascade);

            // 关于隐式外键的说明（注释）：
            // Author 跟 Book 在只写了属性字段之后，EF Core 可以利用自身的"约定机制"（Convention）发现：
            // 1. Author 里有 ICollection<Book>
            // 2. Book 里有 int AuthorId 和 Author Author
            // 这本身已经满足了一对多的隐式契约，EF Core 完全可以直接生成正确的外键结构。
            // 使用 Fluent API (HasMany.WithOne) 有助于处理复杂情况，例如关联关系名称不满足默认约定时，或者我们需要强行修改 OnDelete 删除行为的时候。
        }
    }
}
