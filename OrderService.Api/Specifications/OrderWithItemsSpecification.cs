using Ardalis.Specification;
using OrderService.Api.Models;

namespace OrderService.Api.Specifications;

public class OrderWithItemsSpecification : Specification<Order>
{
    public OrderWithItemsSpecification(int orderId)
    {
        Query.Where(o => o.OrderId == orderId)
            .Include(o => o.OrderItems);
    }
}
