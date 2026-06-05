using TechForge.Core.Dtos;
using TechForge.Core.Querying;

namespace TechForge.Services.Contracts;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetCatalogAsync(ProductQueryOptions options);

    Task<ProductDetailsDto?> GetDetailsAsync(int id);

    Task<IReadOnlyList<ProductDto>> GetFeaturedAsync(int count);

    Task<IReadOnlyList<ProductDto>> GetRelatedAsync(int productId, int count);

    Task<IReadOnlyList<ProductDto>> SearchAsync(string term, int limit);

    Task<IReadOnlyList<string>> GetBrandsAsync();

    Task<ProductInputDto?> GetForEditAsync(int id);

    Task<ProductDto> CreateAsync(ProductInputDto input);

    Task<bool> UpdateAsync(ProductInputDto input);

    Task<bool> DeleteAsync(int id);
}
