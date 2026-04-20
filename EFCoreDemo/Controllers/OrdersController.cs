using Microsoft.AspNetCore.Mvc;
using EFCoreDemo.Application.Services;
using EFCoreDemo.Application.DTOs.Order;

namespace EFCoreDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> PostOrder(CreateOrderRequest request)
        {
            try
            {
                var orderResponse = await _orderService.AddOrderAsync(request);
                return CreatedAtAction(nameof(GetOrder), new { id = orderResponse.Id }, orderResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
}