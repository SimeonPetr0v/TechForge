using NUnit.Framework;
using TechForge.Data;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

[TestFixture]
public class CartServiceTests
{
    private const string UserId = "user-1";

    private static CartService BuildService(out ApplicationDbContext context)
    {
        context = TestDb.NewContext();
        TestDb.SeedUser(context, UserId);
        TestDb.SeedCategory(context);
        TestDb.SeedProduct(context, 1, "GPU", 1000m, stock: 5);
        TestDb.SeedProduct(context, 2, "CPU", 500m, stock: 0); // out of stock
        return new CartService(context, TestDb.CreateMapper());
    }

    [Test]
    public async Task AddAsync_AddsNewItem()
    {
        var service = BuildService(out var context);
        var ok = await service.AddAsync(UserId, productId: 1, quantity: 2);

        Assert.That(ok, Is.True);
        Assert.That(context.CartItems.Single().Quantity, Is.EqualTo(2));
    }

    [Test]
    public async Task AddAsync_MergesQuantityForExistingItem()
    {
        var service = BuildService(out var context);
        await service.AddAsync(UserId, 1, 2);
        await service.AddAsync(UserId, 1, 1);

        Assert.That(context.CartItems.Count(), Is.EqualTo(1));
        Assert.That(context.CartItems.Single().Quantity, Is.EqualTo(3));
    }

    [Test]
    public async Task AddAsync_CapsQuantityAtAvailableStock()
    {
        var service = BuildService(out var context);
        await service.AddAsync(UserId, 1, 99); // stock is only 5

        Assert.That(context.CartItems.Single().Quantity, Is.EqualTo(5));
    }

    [Test]
    public async Task AddAsync_ReturnsFalse_WhenOutOfStock()
    {
        var service = BuildService(out _);
        var ok = await service.AddAsync(UserId, productId: 2, quantity: 1);

        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task AddAsync_ReturnsFalse_WhenProductMissing()
    {
        var service = BuildService(out _);
        var ok = await service.AddAsync(UserId, productId: 999, quantity: 1);

        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task GetCartAsync_ComputesSubtotalAndTotals()
    {
        var service = BuildService(out _);
        await service.AddAsync(UserId, 1, 2); // 2 x 1000 = 2000

        var cart = await service.GetCartAsync(UserId);

        Assert.That(cart.Items, Has.Count.EqualTo(1));
        Assert.That(cart.TotalItems, Is.EqualTo(2));
        Assert.That(cart.Subtotal, Is.EqualTo(2000m));
        Assert.That(cart.IsEmpty, Is.False);
    }

    [Test]
    public async Task GetItemCountAsync_SumsQuantities()
    {
        var service = BuildService(out _);
        await service.AddAsync(UserId, 1, 3);

        var count = await service.GetItemCountAsync(UserId);

        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public async Task UpdateQuantityAsync_ChangesQuantity()
    {
        var service = BuildService(out var context);
        await service.AddAsync(UserId, 1, 1);

        await service.UpdateQuantityAsync(UserId, 1, 4);

        Assert.That(context.CartItems.Single().Quantity, Is.EqualTo(4));
    }

    [Test]
    public async Task UpdateQuantityAsync_RemovesItem_WhenQuantityBelowOne()
    {
        var service = BuildService(out var context);
        await service.AddAsync(UserId, 1, 2);

        await service.UpdateQuantityAsync(UserId, 1, 0);

        Assert.That(context.CartItems.Any(), Is.False);
    }

    [Test]
    public async Task RemoveAsync_RemovesItem()
    {
        var service = BuildService(out var context);
        await service.AddAsync(UserId, 1, 1);

        var ok = await service.RemoveAsync(UserId, 1);

        Assert.That(ok, Is.True);
        Assert.That(context.CartItems.Any(), Is.False);
    }

    [Test]
    public async Task ClearAsync_EmptiesCart()
    {
        var service = BuildService(out var context);
        await service.AddAsync(UserId, 1, 1);

        await service.ClearAsync(UserId);

        Assert.That(context.CartItems.Any(), Is.False);
    }
}
