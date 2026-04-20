using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetOrderWithDetailsAsync(Guid id)
        {
            return await _context.Orders
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Product)
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
        {
            return await _context.Orders
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Product)
                                 .ToListAsync();
        }
    }
}