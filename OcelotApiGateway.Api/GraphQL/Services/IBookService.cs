using OcelotApiGateway.Api.GraphQL.Models;

namespace OcelotApiGateway.Api.GraphQL.Services;

public interface IBookService
{
    Task<List<Book>> GetBooksAsync(IHttpClientFactory httpClientFactory);

}
