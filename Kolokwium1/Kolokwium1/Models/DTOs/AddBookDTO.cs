namespace Kolokwium1.Models.DTOs;

public class AddBookDTO
{
    public string? Title { get; set; }
    public List<Author> Authors { get; set; } = null!;
}