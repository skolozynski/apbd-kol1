using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Repositories;

public interface IBookRepository
{
    Task<bool> DoesBookExist(int id);
    Task<BookWithAuthorsDTO> GetBook(int id);
}