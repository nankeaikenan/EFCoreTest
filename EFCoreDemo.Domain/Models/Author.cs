using System.Collections.Generic;

namespace EFCoreDemo.Domain.Models
{
    // 一对多：作者实体 (主表)
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // 导航属性：一个作者可以写“多本”书，所以用集合 ICollection/List
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
