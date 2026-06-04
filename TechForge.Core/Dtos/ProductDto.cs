namespace TechForge.Core.Dtos;

public class ProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public double Rating { get; set; }

    public int StockQuantity { get; set; }

    public bool InStock => StockQuantity > 0;

    public bool IsFeatured { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
}
