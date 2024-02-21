using Ardalis.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Models;
using OrderService.Api.Repository;
using OrderService.Api.Specifications;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IRepositoryBase<Order> _orderRepository;

        public OrdersController(IRepositoryBase<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }


        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            await _orderRepository.AddAsync(order);
            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }


        [HttpGet("ByCustomer/{customerId}")]
        public async Task<ActionResult<List<Order>>> GetOrdersByCustomer(string customerId)
        {
            var spec = new OrdersByCustomerSpecification(customerId);
            var orders = await _orderRepository.ListAsync(spec);

            if (orders == null || !orders.Any())
            {
                return NotFound();
            }

            return Ok(orders);
        }
    }
}
