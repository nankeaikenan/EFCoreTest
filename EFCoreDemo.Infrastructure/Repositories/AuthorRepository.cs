using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EFCoreDemo.Infrastructure.Repositories
{
    public class AuthorRepository : Repository<Author>, IAuthorRepository
    {
        public AuthorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Author>> GetAuthorsWithBooksAsync()
        {
            return await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();
        }
    }
}
