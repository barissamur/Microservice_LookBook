using OcelotApiGateway.Api.GraphQL.Models;
using System.Net.Http;

namespace OcelotApiGateway.Api.GraphQL.Services
{
    public class BookService : IBookService
    {
        public async Task<List<Book>> GetBooksAsync([Service] IHttpClientFactory httpClientFactory)
        {
            var client = httpClientFactory.CreateClient("BookSer");
            var response = await client.GetAsync("Books");
            response.EnsureSuccessStatusCode();

            var books = await response.Content.ReadFromJsonAsync<IEnumerable<Book>>();

            return books.ToList();
        }
    }
}
