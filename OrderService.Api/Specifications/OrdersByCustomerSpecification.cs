using Ardalis.Specification;
using OrderService.Api.Models;

namespace OrderService.Api.Specifications;

public class OrdersByCustomerSpecification : Specification<Order>
{
    public OrdersByCustomerSpecification(string customerId)
    {
        Query.Where(o => o.CustomerId == customerId);
    }
}
