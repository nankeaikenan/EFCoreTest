using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using EFCoreDemo.Infrastructure.Data;

namespace EFCoreDemo.Infrastructure.Repositories
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}