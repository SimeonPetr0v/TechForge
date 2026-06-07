using TechForge.Core.Dtos;
using TechForge.Core.Querying;

namespace TechForge.Web.ViewModels;

public class CatalogViewModel
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public string? Brand { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool InStockOnly { get; set; }

    public ProductSortOption Sort { get; set; } = ProductSortOption.Newest;

    public int Page { get; set; } = 1;

    public PagedResult<ProductDto> Products { get; set; } = new();

    public IReadOnlyList<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();

    public IReadOnlyList<string> Brands { get; set; } = Array.Empty<string>();

    public IDictionary<string, string?> RouteValues()
    {
        var values = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(Search)) values["Search"] = Search;
        if (CategoryId.HasValue) values["CategoryId"] = CategoryId.Value.ToString();
        if (!string.IsNullOrWhiteSpace(Brand)) values["Brand"] = Brand;
        if (MinPrice.HasValue) values["MinPrice"] = MinPrice.Value.ToString();
        if (MaxPrice.HasValue) values["MaxPrice"] = MaxPrice.Value.ToString();
        if (InStockOnly) values["InStockOnly"] = "true";
        if (Sort != ProductSortOption.Newest) values["Sort"] = Sort.ToString();

        return values;
    }
}
