using BookService.Api.Models;
using MongoDB.Driver;

namespace BookService.Api.Repository;

public class BookRepository
{
    private readonly IMongoCollection<Book> _books;

    public BookRepository(IMongoDatabase database)
    {
        _books = database.GetCollection<Book>("Books");
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _books.Find(book => true).ToListAsync();
    }

    public async Task<Book> GetByIdAsync(string id)
    {
        return await _books.Find<Book>(book => book.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Book book)
    {
        await _books.InsertOneAsync(book);
    } 
}
