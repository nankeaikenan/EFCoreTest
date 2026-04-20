using System.Text.Json.Serialization;

namespace EFCoreDemo.Domain.Models
{
    // 一对多：书籍实体 (从表)
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        // 【隐式外键关联字段】
        // EF Core 同样依靠约定，根据这里关联的属性名称 Author + Id => 自动识别出外键 AuthorId
        public int AuthorId { get; set; }

        // 导航属性：一本书只属于“一位”作者
        [JsonIgnore]
        public Author? Author { get; set; }
    }
}
