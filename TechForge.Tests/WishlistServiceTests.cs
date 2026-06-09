using NUnit.Framework;
using TechForge.Data;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

[TestFixture]
public class WishlistServiceTests
{
    private const string UserId = "user-1";

    private static WishlistService BuildService(out ApplicationDbContext context)
    {
        context = TestDb.NewContext();
        TestDb.SeedUser(context, UserId);
        TestDb.SeedCategory(context);
        TestDb.SeedProduct(context, 1, "GPU", 1000m);
        TestDb.SeedProduct(context, 2, "CPU", 500m);
        return new WishlistService(context, TestDb.CreateMapper());
    }

    [Test]
    public async Task ToggleAsync_AddsWhenAbsent_ReturnsTrue()
    {
        var service = BuildService(out var context);
        var added = await service.ToggleAsync(UserId, 1);

        Assert.That(added, Is.True);
        Assert.That(context.WishlistItems.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ToggleAsync_RemovesWhenPresent_ReturnsFalse()
    {
        var service = BuildService(out var context);
        await service.ToggleAsync(UserId, 1);

        var stillIn = await service.ToggleAsync(UserId, 1);

        Assert.That(stillIn, Is.False);
        Assert.That(context.WishlistItems.Any(), Is.False);
    }

    [Test]
    public async Task ToggleAsync_ReturnsFalse_WhenProductMissing()
    {
        var service = BuildService(out _);
        Assert.That(await service.ToggleAsync(UserId, 999), Is.False);
    }

    [Test]
    public async Task IsInWishlistAsync_ReflectsState()
    {
        var service = BuildService(out _);
        Assert.That(await service.IsInWishlistAsync(UserId, 1), Is.False);

        await service.ToggleAsync(UserId, 1);

        Assert.That(await service.IsInWishlistAsync(UserId, 1), Is.True);
    }

    [Test]
    public async Task GetWishlistAsync_ReturnsSavedProducts()
    {
        var service = BuildService(out _);
        await service.ToggleAsync(UserId, 1);
        await service.ToggleAsync(UserId, 2);

        var items = await service.GetWishlistAsync(UserId);

        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items.All(i => i.ProductName.Length > 0), Is.True);
    }

    [Test]
    public async Task RemoveAsync_RemovesItem()
    {
        var service = BuildService(out var context);
        await service.ToggleAsync(UserId, 1);

        var ok = await service.RemoveAsync(UserId, 1);

        Assert.That(ok, Is.True);
        Assert.That(context.WishlistItems.Any(), Is.False);
    }

    [Test]
    public async Task GetCountAsync_ReturnsNumberOfItems()
    {
        var service = BuildService(out _);
        await service.ToggleAsync(UserId, 1);
        await service.ToggleAsync(UserId, 2);

        Assert.That(await service.GetCountAsync(UserId), Is.EqualTo(2));
    }
}
