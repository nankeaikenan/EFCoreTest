using EFCoreDemo.Application.DTOs.Order;

namespace EFCoreDemo.Application.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetOrdersAsync();
        Task<OrderResponse?> GetOrderByIdAsync(Guid id);
        Task<OrderResponse> AddOrderAsync(CreateOrderRequest request);
    }
}