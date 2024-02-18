using BookService.Api.Models;
using BookService.Api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BookService.Api.Controller;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly BookRepository _bookRepository;


    public BooksController(BookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }


    [HttpGet("{id:length(24)}", Name = "GetBook")]
    public async Task<ActionResult<Book>> GetById(string id)
    {
        var book = await _bookRepository.GetByIdAsync(id);

        if (book == null)
            return NotFound();


        return book;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _bookRepository.GetAllAsync();
        return Ok(books);
    }


    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        await _bookRepository.CreateAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

}
