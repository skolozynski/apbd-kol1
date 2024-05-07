using Kolokwium1.Models.DTOs;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.Data.SqlClient;

namespace Kolokwium1.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IConfiguration _configuration;

    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM books WHERE PK = @id";
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res != null;
    }

    public async Task<BookWithAuthorsDTO> GetBook(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        var query = "SELECT " +
                    "books.PK AS bookID, " +
                    "books.title AS title, " +
                    "authors.first_name AS firstName, " +
                    "authors.last_name AS lastName " +
                    "FROM books " +
                    "JOIN books_authors ON books.PK = books_authors.FK_book " +
                    "JOIN authors ON books_authors.FK_author = authors.PK " +
                    "WHERE books.PK = @id";
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();
        var bookId = reader.GetOrdinal("BookID");
        var title = reader.GetOrdinal("title");
        var firstName = reader.GetOrdinal("firstName");
        var lastName = reader.GetOrdinal("lastName");

        BookWithAuthorsDTO bookWithAuthorsDto = null;
        while (await reader.ReadAsync())
        {
            if (bookWithAuthorsDto is not null)
            {
                bookWithAuthorsDto.Authors.Add(new Author()
                {
                    FirstName = reader.GetString(firstName),
                    LastName = reader.GetString(lastName)
                });

            }
            else
            {
                bookWithAuthorsDto = new BookWithAuthorsDTO()
                {
                    Id = reader.GetInt32(bookId),
                    Title = reader.GetString(title),
                    Authors = new List<Author>()
                    {
                        new Author()
                        {
                            FirstName = reader.GetString(firstName),
                            LastName = reader.GetString(lastName)
                        }
                    }
                };
            }
        }
        return bookWithAuthorsDto ?? throw new InvalidOperationException();
    }
}