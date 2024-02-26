using Microsoft.EntityFrameworkCore;
using OrderService.Api.IRepo;
using OrderService.Api.Models;
using OrderService.Api.Specifications;

namespace BookService.Api.Areas.GraphQL.Queries;

public class Query
{
    public async Task<List<Order>> GetOrders([Service] IRepository<Order> repository)
    {
        return await repository.ListAsync();
    }


    public async Task<Order> GetOrderWithItems([Service] IRepository<Order> repository, int orderId)
    {
        var spec = new OrderWithItemsSpecification(orderId);
        var resp = await repository.FirstOrDefaultAsync(spec);
        return resp;
    }
}
