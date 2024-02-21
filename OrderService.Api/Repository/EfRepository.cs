using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.IRepo;

namespace OrderService.Api.Repository;
public class EfRepository<T> : RepositoryBase<T>, IReadRepositoryBase<T>, IRepository<T> where T : class
{
    public EfRepository(OrderContext dbContext) : base(dbContext)
    {

    }
}
