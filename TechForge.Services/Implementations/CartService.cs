using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Data;
using TechForge.Services.Contracts;

namespace TechForge.Services.Implementations;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CartService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var items = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .OrderBy(ci => ci.AddedOn)
            .AsNoTracking()
            .ToListAsync();

        return new CartDto { Items = _mapper.Map<List<CartItemDto>>(items) };
    }

    public async Task<int> GetItemCountAsync(string userId)
    {
        return await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .SumAsync(ci => (int?)ci.Quantity) ?? 0;
    }

    public async Task<bool> AddAsync(string userId, int productId, int quantity)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        var product = await _context.Products.FindAsync(productId);
        if (product is null || product.StockQuantity <= 0)
        {
            return false;
        }

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (existing is null)
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = Math.Min(quantity, product.StockQuantity)
            });
        }
        else
        {
            existing.Quantity = Math.Min(existing.Quantity + quantity, product.StockQuantity);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuantityAsync(string userId, int productId, int quantity)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (existing is null)
        {
            return false;
        }

        if (quantity < 1)
        {
            _context.CartItems.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        var product = await _context.Products.FindAsync(productId);
        var max = product?.StockQuantity ?? quantity;
        existing.Quantity = Math.Min(quantity, max);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(string userId, int productId)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (existing is null)
        {
            return false;
        }

        _context.CartItems.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (items.Count == 0)
        {
            return;
        }

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
