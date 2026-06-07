using TechForge.Core.Dtos;

namespace TechForge.Web.ViewModels;

public class ProductDetailsViewModel
{
    public ProductDetailsDto Product { get; set; } = null!;

    public IReadOnlyList<ProductDto> Related { get; set; } = Array.Empty<ProductDto>();

    public bool IsAuthenticated { get; set; }

    public bool IsAdmin { get; set; }

    public bool HasReviewed { get; set; }

    public string? CurrentUserId { get; set; }
}
