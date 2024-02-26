using Ardalis.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.IRepo;
using OrderService.Api.Models;
using OrderService.Api.Repository;
using OrderService.Api.Specifications;

namespace OrderService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;

    public OrdersController(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }


    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<Order>> PostOrder(Order order)
    {
        await _orderRepository.AddAsync(order);
        await _orderItemRepository.AddRangeAsync(order.OrderItems);
        return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
    }


    //[HttpGet("{id}")]
    //public async Task<ActionResult<Order>> GetOrder(int id)
    //{
    //    var order = await _orderRepository.GetByIdAsync(id);

    //    if (order == null)
    //    {
    //        return NotFound();
    //    }

    //    return order;
    //}

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderWithItems(int id)
    {
        var spec = new OrderWithItemsSpecification(id);
        var order = await _orderRepository.GetBySpecAsync(spec);

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
