using TechForge.Core.Dtos;

namespace TechForge.Services.Contracts;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetAllAsync();

    Task<IReadOnlyList<ReviewDto>> GetForProductAsync(int productId);

    Task<ReviewDto?> AddAsync(int productId, string userId, int rating, string comment);

    Task<bool> DeleteAsync(int reviewId, string userId, bool isAdmin);

    Task<bool> HasUserReviewedAsync(int productId, string userId);
}
