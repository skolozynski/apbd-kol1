namespace Kolokwium1.Models.DTOs;

public class BookWithAuthorsDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public List<Author> Authors { get; set; } = null!;

}

public class Author
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
