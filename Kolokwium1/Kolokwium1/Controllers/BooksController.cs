using Kolokwium1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers;

[ApiController]
[Route("/api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BooksController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    [HttpGet("/api/books/{id}/authors")]
    public async Task<IActionResult> GetBookAuthors(int id)
    {
        if (!await _bookRepository.DoesBookExist(id))
            return NotFound($"Book with id {id} does not exist");

        var res = await _bookRepository.GetBook(id);
        return Ok(res);
    }
    
    
}