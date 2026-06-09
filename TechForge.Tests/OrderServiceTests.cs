using NUnit.Framework;
using TechForge.Core.Entities;
using TechForge.Core.Enums;
using TechForge.Data;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

[TestFixture]
public class OrderServiceTests
{
    private const string UserId = "user-1";

    private static OrderService BuildService(out ApplicationDbContext context, bool withCart = true)
    {
        context = TestDb.NewContext();
        TestDb.SeedUser(context, UserId);
        TestDb.SeedCategory(context);
        TestDb.SeedProduct(context, 1, "GPU", 1000m, stock: 5);
        TestDb.SeedProduct(context, 2, "CPU", 500m, stock: 10);

        if (withCart)
        {
            context.CartItems.Add(new CartItem { UserId = UserId, ProductId = 1, Quantity = 2 });
            context.CartItems.Add(new CartItem { UserId = UserId, ProductId = 2, Quantity = 1 });
            context.SaveChanges();
        }

        return new OrderService(context, TestDb.CreateMapper());
    }

    [Test]
    public async Task PlaceOrderAsync_CreatesOrder_WithCorrectTotal()
    {
        var service = BuildService(out _);

        var order = await service.PlaceOrderAsync(UserId, "12 Test Street");

        Assert.That(order, Is.Not.Null);
        Assert.That(order!.TotalPrice, Is.EqualTo(2500m)); // 2*1000 + 1*500
        Assert.That(order.Items, Has.Count.EqualTo(2));
        Assert.That(order.Status, Is.EqualTo(nameof(OrderStatus.Pending)));
    }

    [Test]
    public async Task PlaceOrderAsync_DecrementsStock()
    {
        var service = BuildService(out var context);

        await service.PlaceOrderAsync(UserId, "12 Test Street");

        Assert.That(context.Products.Single(p => p.Id == 1).StockQuantity, Is.EqualTo(3)); // 5 - 2
        Assert.That(context.Products.Single(p => p.Id == 2).StockQuantity, Is.EqualTo(9)); // 10 - 1
    }

    [Test]
    public async Task PlaceOrderAsync_ClearsCart()
    {
        var service = BuildService(out var context);

        await service.PlaceOrderAsync(UserId, "12 Test Street");

        Assert.That(context.CartItems.Any(ci => ci.UserId == UserId), Is.False);
    }

    [Test]
    public async Task PlaceOrderAsync_ReturnsNull_WhenCartEmpty()
    {
        var service = BuildService(out _, withCart: false);

        var order = await service.PlaceOrderAsync(UserId, "12 Test Street");

        Assert.That(order, Is.Null);
    }

    [Test]
    public async Task GetUserOrdersAsync_ReturnsUsersOrders()
    {
        var service = BuildService(out _);
        await service.PlaceOrderAsync(UserId, "12 Test Street");

        var orders = await service.GetUserOrdersAsync(UserId);

        Assert.That(orders, Has.Count.EqualTo(1));
        Assert.That(orders[0].Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsOrder_ForOwner_AndNullForOthers()
    {
        var service = BuildService(out _);
        var placed = await service.PlaceOrderAsync(UserId, "12 Test Street");

        var mine = await service.GetByIdAsync(placed!.Id, UserId);
        var someoneElses = await service.GetByIdAsync(placed.Id, "another-user");

        Assert.That(mine, Is.Not.Null);
        Assert.That(someoneElses, Is.Null);
    }

    [Test]
    public async Task UpdateStatusAsync_ChangesStatus()
    {
        var service = BuildService(out _);
        var placed = await service.PlaceOrderAsync(UserId, "12 Test Street");

        var ok = await service.UpdateStatusAsync(placed!.Id, OrderStatus.Shipped);
        var updated = await service.GetByIdAsync(placed.Id, UserId);

        Assert.That(ok, Is.True);
        Assert.That(updated!.Status, Is.EqualTo(nameof(OrderStatus.Shipped)));
    }
}
