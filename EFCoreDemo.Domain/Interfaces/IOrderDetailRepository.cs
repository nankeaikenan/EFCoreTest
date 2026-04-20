using EFCoreDemo.Domain.Models;

namespace EFCoreDemo.Domain.Interfaces
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        // 可以添加 OrderDetail 特有的方法
    }
}