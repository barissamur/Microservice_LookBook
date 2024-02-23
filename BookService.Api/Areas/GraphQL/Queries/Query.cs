using BookService.Api.Models;
using BookService.Api.Repository;
using MongoDB.Driver;

namespace BookService.Api.Areas.GraphQL.Queries;

public class Query
{
    private readonly BookRepository _context;

    public Query(BookRepository context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetBooks() => await _context.GetAllAsync();


    public async Task<Book> GetBookById(string id) => await _context.GetByIdAsync(id);


    public async Task<List<Book>> GetBooksByIdsAsync(IEnumerable<string> ids) => await _context.GetBooksByIdsAsync(ids);
}
