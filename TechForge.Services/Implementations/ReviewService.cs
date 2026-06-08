using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Data;
using TechForge.Services.Contracts;

namespace TechForge.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ReviewService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetAllAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedOn)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<ReviewDto>>(reviews);
    }

    public async Task<IReadOnlyList<ReviewDto>> GetForProductAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedOn)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<ReviewDto>>(reviews);
    }

    public async Task<ReviewDto?> AddAsync(int productId, string userId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            return null;
        }

        var product = await _context.Products.FindAsync(productId);
        if (product is null)
        {
            return null;
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedOn = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await RecalculateRatingAsync(product);

        var saved = await _context.Reviews
            .Include(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        return saved is null ? null : _mapper.Map<ReviewDto>(saved);
    }

    public async Task<bool> DeleteAsync(int reviewId, string userId, bool isAdmin)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review is null)
        {
            return false;
        }

        if (!isAdmin && review.UserId != userId)
        {
            return false;
        }

        var product = await _context.Products.FindAsync(review.ProductId);

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        if (product is not null)
        {
            await RecalculateRatingAsync(product);
        }

        return true;
    }

    public async Task<bool> HasUserReviewedAsync(int productId, string userId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
    }

    private async Task RecalculateRatingAsync(Product product)
    {
        var ratings = await _context.Reviews
            .Where(r => r.ProductId == product.Id)
            .Select(r => r.Rating)
            .ToListAsync();

        product.Rating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0;
        await _context.SaveChangesAsync();
    }
}
