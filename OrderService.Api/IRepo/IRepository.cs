using Ardalis.Specification;

namespace OrderService.Api.IRepo;

public interface IRepository<T> : IRepositoryBase<T> where T : class
{
}
