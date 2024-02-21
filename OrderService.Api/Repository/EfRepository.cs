using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Api.Repository;
public class EfRepository<T> : RepositoryBase<T> where T : class
{
    protected readonly DbContext _dbContext;

    public EfRepository(DbContext dbContext, ISpecificationEvaluator specificationEvaluator) : base(dbContext, specificationEvaluator)
    {
        _dbContext = dbContext;
    }
}
