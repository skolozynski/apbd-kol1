using Kolokwium1.Models.DTOs;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.AspNetCore.Http.HttpResults;
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
    
    public async Task<bool> DoesAuthorExist(string? firstName, string? lastName)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM authors WHERE " +
                              "first_name = @firstName AND " +
                              "last_name = @lastName";
        command.Parameters.AddWithValue("@firstName", firstName);
        command.Parameters.AddWithValue("@lastName", lastName);
        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res != null;
    }

    public async Task<BookWithAuthorsDTO> AddBook(AddBookDTO addBookDto)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "INSERT INTO books VALUES(@title); SELECT @@IDENTITY AS ID;";
        command.Parameters.AddWithValue("@title", addBookDto.Title);

        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        object? id;
        try
        {
            id  = await command.ExecuteScalarAsync();
            command.Parameters.Clear();
            
            foreach (var author in addBookDto.Authors)
            {
                var authorId = GetAuthorId(author);
                command.CommandText = "INSERT INTO books_authors VALUES(@id, @authorID)";
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@authorID", authorId);
                command.Parameters.Clear();
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

        BookWithAuthorsDTO res = new BookWithAuthorsDTO()
        {
            Id = (int)id,
            Title = addBookDto.Title,
            Authors = addBookDto.Authors
        };

        return res;
    }

    public async Task<int> GetAuthorId(Author author)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT PK as ID FROM authors WHERE first_name = @firstName AND last_name = @lastName";
        command.Parameters.AddWithValue("@firstName", author.FirstName);
        command.Parameters.AddWithValue("@firstName", author.LastName);

        await connection.OpenAsync();
        var reader = await command.ExecuteReaderAsync();
        var authorId = reader.GetOrdinal("ID");
        
        object? id = null;

        while( await reader.ReadAsync())
        {
            id = reader.GetInt32(authorId);
        }
        return (int)(id ?? throw new InvalidOperationException());
    }
}