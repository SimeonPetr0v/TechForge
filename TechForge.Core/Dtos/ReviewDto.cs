namespace TechForge.Core.Dtos;

public class ReviewDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string AuthorId { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public string AuthorName { get; set; } = string.Empty;
}
