using EFCoreDemo.Application.DTOs.Order;
using EFCoreDemo.Domain.Interfaces;
using EFCoreDemo.Domain.Models;
using AutoMapper;

namespace EFCoreDemo.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IRepository<Product> productRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersWithDetailsAsync();
            return _mapper.Map<IEnumerable<OrderResponse>>(orders);
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            return order == null ? null : _mapper.Map<OrderResponse>(order);
        }

        public async Task<OrderResponse> AddOrderAsync(CreateOrderRequest request)
        {
            if (request.OrderDetails == null || !request.OrderDetails.Any())
                throw new ArgumentException("订单必须包含至少一个商品。");

            var order = new Order
            {
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                ShippingAddress = request.ShippingAddress,
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",  // 业务逻辑在 Service 层处理
                OrderDate = DateTime.UtcNow
            };

            // 根据当前商品价格计算每行明细
            decimal total = 0;
            foreach (var detailRequest in request.OrderDetails)
            {
                var product = await _productRepository.GetByIdAsync(detailRequest.ProductId)
                    ?? throw new ArgumentException($"商品 ID {detailRequest.ProductId} 不存在。");

                var detail = new OrderDetail
                {
                    ProductId = detailRequest.ProductId,
                    Quantity = detailRequest.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = detailRequest.Quantity * product.Price
                };
                total += detail.Subtotal;
                order.OrderDetails.Add(detail);
            }
            order.TotalAmount = total;

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();
            return _mapper.Map<OrderResponse>(order);
        }
    }
}