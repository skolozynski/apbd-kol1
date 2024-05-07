using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Repositories;

public interface IBookRepository
{
    Task<bool> DoesBookExist(int id);
    Task<BookWithAuthorsDTO> GetBook(int id);
    Task<bool> DoesAuthorExist(string? firstName, string? lastName);

    Task<BookWithAuthorsDTO> AddBook(AddBookDTO addBookDto);
}