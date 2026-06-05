using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Core.Enums;
using TechForge.Data;
using TechForge.Services.Contracts;

namespace TechForge.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrderDto?> PlaceOrderAsync(string userId, string shippingAddress)
    {
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return null;
        }

        var order = new Order
        {
            UserId = userId,
            OrderedOn = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress
        };

        foreach (var cartItem in cartItems)
        {
            var product = cartItem.Product;
            if (product is null)
            {
                continue;
            }

            var quantity = Math.Min(cartItem.Quantity, product.StockQuantity);
            if (quantity <= 0)
            {
                continue;
            }

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                Price = product.Price
            });

            product.StockQuantity -= quantity;
        }

        if (order.Items.Count == 0)
        {
            return null;
        }

        order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        var saved = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        return saved is null ? null : _mapper.Map<OrderDto>(saved);
    }

    public async Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderedOn)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(int orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? null : _mapper.Map<OrderDto>(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderedOn)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
        {
            return false;
        }

        order.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }
}
