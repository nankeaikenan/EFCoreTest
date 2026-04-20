using EFCoreDemo.Domain.Models;

namespace EFCoreDemo.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(Guid id);
        Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();
    }
}