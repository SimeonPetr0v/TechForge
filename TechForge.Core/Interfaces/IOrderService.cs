using TechForge.Core.Dtos;
using TechForge.Core.Enums;

namespace TechForge.Core.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> PlaceOrderAsync(string userId, string shippingAddress);

    Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(string userId);

    Task<OrderDto?> GetByIdAsync(int orderId, string userId);

    Task<IReadOnlyList<OrderDto>> GetAllAsync();

    Task<bool> UpdateStatusAsync(int orderId, OrderStatus status);
}
