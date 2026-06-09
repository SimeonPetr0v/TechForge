using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Core.Querying;
using TechForge.Data;
using TechForge.Services.Contracts;

namespace TechForge.Services.Implementations;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetCatalogAsync(ProductQueryOptions options)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(options.SearchTerm))
        {
            var term = options.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Brand.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        if (options.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == options.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(options.Brand))
        {
            query = query.Where(p => p.Brand == options.Brand);
        }

        if (options.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= options.MinPrice.Value);
        }

        if (options.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= options.MaxPrice.Value);
        }

        if (options.InStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        query = options.Sort switch
        {
            ProductSortOption.PriceLowToHigh => query.OrderBy(p => p.Price),
            ProductSortOption.PriceHighToLow => query.OrderByDescending(p => p.Price),
            ProductSortOption.Rating => query.OrderByDescending(p => p.Rating),
            ProductSortOption.Popularity => query.OrderByDescending(p => p.Reviews.Count),
            ProductSortOption.NameAToZ => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedOn)
        };

        var page = options.Page < 1 ? 1 : options.Page;
        var size = options.PageSize < 1 ? 12 : options.PageSize;

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            Page = page,
            PageSize = size,
            TotalCount = total
        };
    }

    public async Task<ProductDetailsDto?> GetDetailsAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : _mapper.Map<ProductDetailsDto>(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetFeaturedAsync(int count)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<IReadOnlyList<ProductDto>> GetRelatedAsync(int productId, int count)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product is null)
        {
            return Array.Empty<ProductDto>();
        }

        var related = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<ProductDto>>(related);
    }

    public async Task<IReadOnlyList<ProductDto>> SearchAsync(string term, int limit)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Array.Empty<ProductDto>();
        }

        var t = term.Trim().ToLower();
        var items = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Name.ToLower().Contains(t) || p.Brand.ToLower().Contains(t))
            .OrderByDescending(p => p.Rating)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<ProductDto>>(items);
    }

    public async Task<IReadOnlyList<string>> GetBrandsAsync()
    {
        return await _context.Products
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
    }

    public async Task<ProductInputDto?> GetForEditAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product is null ? null : _mapper.Map<ProductInputDto>(product);
    }

    public async Task<ProductDto> CreateAsync(ProductInputDto input)
    {
        var product = _mapper.Map<Product>(input);
        product.Id = 0;
        product.CreatedOn = DateTime.UtcNow;
        product.Rating = 0;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> UpdateAsync(ProductInputDto input)
    {
        var product = await _context.Products.FindAsync(input.Id);
        if (product is null)
        {
            return false;
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Brand = input.Brand;
        product.Price = input.Price;
        product.StockQuantity = input.StockQuantity;
        product.ImageUrl = input.ImageUrl;
        product.CategoryId = input.CategoryId;
        product.Specifications = input.Specifications;
        product.IsFeatured = input.IsFeatured;
        product.ReleaseDate = input.ReleaseDate;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
