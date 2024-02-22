using OcelotApiGateway.Api.GraphQL.Models;
using OcelotApiGateway.Api.GraphQL.Services;
using OcelotApiGateway.Api.GraphQL.Types;

namespace OcelotApiGateway.Api.GraphQL.Queries;

// GraphQL/Queries/Query.cs
public class Query
{
    public async Task<IEnumerable<Book>> GetBooks([Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("BookSer");
        var response = await client.GetAsync("Books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<IEnumerable<Book>>();

        return books;
    }
}
