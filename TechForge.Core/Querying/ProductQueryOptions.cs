namespace TechForge.Core.Querying;

public class ProductQueryOptions
{
    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }

    public string? Brand { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool InStockOnly { get; set; }

    public ProductSortOption Sort { get; set; } = ProductSortOption.Newest;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}
