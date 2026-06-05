using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Data;
using TechForge.Services.Contracts;

namespace TechForge.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public WishlistService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WishlistItemDto>> GetWishlistAsync(string userId)
    {
        var items = await _context.WishlistItems
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedOn)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<WishlistItemDto>>(items);
    }

    public async Task<bool> ToggleAsync(string userId, int productId)
    {
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing is not null)
        {
            _context.WishlistItems.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }

        var product = await _context.Products.FindAsync(productId);
        if (product is null)
        {
            return false;
        }

        _context.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInWishlistAsync(string userId, int productId)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<bool> RemoveAsync(string userId, int productId)
    {
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (existing is null)
        {
            return false;
        }

        _context.WishlistItems.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountAsync(string userId)
    {
        return await _context.WishlistItems.CountAsync(w => w.UserId == userId);
    }
}
