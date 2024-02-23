using BookService.Api.Models;
using BookService.Api.Repository;
using MongoDB.Driver;

namespace BookService.Api.Areas.GraphQL.Queries;

public class Query
{
    public async Task<List<Book>> GetBooks([Service] BookRepository context) => await context.GetAllAsync();
}
