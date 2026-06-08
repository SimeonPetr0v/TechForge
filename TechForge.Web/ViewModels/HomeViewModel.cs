using TechForge.Core.Dtos;

namespace TechForge.Web.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<ProductDto> Featured { get; set; } = Array.Empty<ProductDto>();

    public IReadOnlyList<ProductDto> Newest { get; set; } = Array.Empty<ProductDto>();

    public IReadOnlyList<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();
}
