using EFCoreDemo.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EFCoreDemo.Domain.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<IEnumerable<Author>> GetAuthorsWithBooksAsync();
    }
}