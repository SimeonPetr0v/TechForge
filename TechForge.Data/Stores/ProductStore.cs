using Microsoft.EntityFrameworkCore;
using TechForge.Core.Entities;
using TechForge.Core.Interfaces;

namespace TechForge.Data.Stores;

public class ProductStore : Store<Product>, IProductStore
{
    public ProductStore(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await DbSet
            .Include(p => p.Category)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int categoryId, int count)
    {
        return await DbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.Id != productId)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public IQueryable<Product> QueryWithCategory()
    {
        return DbSet.Include(p => p.Category).AsQueryable();
    }
}
