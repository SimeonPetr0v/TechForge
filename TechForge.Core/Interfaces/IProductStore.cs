using TechForge.Core.Entities;

namespace TechForge.Core.Interfaces;

public interface IProductStore : IStore<Product>
{
    Task<Product?> GetByIdWithDetailsAsync(int id);

    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count);

    Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int categoryId, int count);

    IQueryable<Product> QueryWithCategory();
}
