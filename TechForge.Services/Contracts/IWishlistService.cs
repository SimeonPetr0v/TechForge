using TechForge.Core.Dtos;

namespace TechForge.Services.Contracts;

public interface IWishlistService
{
    Task<IReadOnlyList<WishlistItemDto>> GetWishlistAsync(string userId);

    Task<bool> ToggleAsync(string userId, int productId);

    Task<bool> IsInWishlistAsync(string userId, int productId);

    Task<bool> RemoveAsync(string userId, int productId);

    Task<int> GetCountAsync(string userId);
}
