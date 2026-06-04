namespace TechForge.Core.Dtos;

public class ProductInputDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public string? Specifications { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime ReleaseDate { get; set; }
}
