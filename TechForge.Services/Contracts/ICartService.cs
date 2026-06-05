using TechForge.Core.Dtos;

namespace TechForge.Services.Contracts;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);

    Task<int> GetItemCountAsync(string userId);

    Task<bool> AddAsync(string userId, int productId, int quantity);

    Task<bool> UpdateQuantityAsync(string userId, int productId, int quantity);

    Task<bool> RemoveAsync(string userId, int productId);

    Task ClearAsync(string userId);
}
