using BookService.Api.Models;
using BookService.Api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BookService.Api.Controller;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly BookRepository _bookRepository;
    private readonly ILogger<BooksController> _logger;

    public BooksController(BookRepository bookRepository
        , ILogger<BooksController> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }


    [HttpGet("{id:length(24)}", Name = "GetBook")]
    public async Task<ActionResult<Book>> GetById(string id)
    {
        _logger.LogInformation("Veri çekme isteği. Kullanıcı id: {Identity}", User.Claims.FirstOrDefault().Value);

        var book = await _bookRepository.GetByIdAsync(id);

        if (book == null)
        {
            _logger.LogWarning("Veri bulunamadı. Kullanıcı id: {Identity}", User.Claims.FirstOrDefault().Value);

            return NotFound();
        }

        _logger.LogWarning("Veri çekme işlemi başarılı. Kullanıcı id: {Identity}", User.Claims.FirstOrDefault().Value);
        return book;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = HttpContext.Request.Headers;
        var userId2 = HttpContext.Request.Headers["X-User-Id"];

        _logger.LogInformation("Veri çekme isteği. Kullanıcı id: {Identity}", userId);
        var books = await _bookRepository.GetAllAsync();


        _logger.LogWarning("Veri çekme işlemi başarılı. Kullanıcı id: {Identity}", userId);
        return Ok(books);
    }


    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        await _bookRepository.CreateAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

}
