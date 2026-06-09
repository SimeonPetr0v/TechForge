using NUnit.Framework;
using TechForge.Core.Dtos;
using TechForge.Data;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

[TestFixture]
public class CategoryServiceTests
{
    private static CategoryService BuildService(out ApplicationDbContext context)
    {
        context = TestDb.NewContext();
        TestDb.SeedCategory(context, 1, "Graphics Cards");
        TestDb.SeedCategory(context, 2, "Processors");
        TestDb.SeedProduct(context, 1, "GPU", 1000m, categoryId: 1);
        return new CategoryService(context, TestDb.CreateMapper());
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllCategories_WithProductCounts()
    {
        var service = BuildService(out _);
        var categories = await service.GetAllAsync();

        Assert.That(categories, Has.Count.EqualTo(2));
        var gpu = categories.Single(c => c.Name == "Graphics Cards");
        Assert.That(gpu.ProductCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsCategory_WhenExists()
    {
        var service = BuildService(out _);
        var category = await service.GetByIdAsync(2);

        Assert.That(category, Is.Not.Null);
        Assert.That(category!.Name, Is.EqualTo("Processors"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var service = BuildService(out _);
        Assert.That(await service.GetByIdAsync(999), Is.Null);
    }

    [Test]
    public async Task CreateAsync_AddsCategory()
    {
        var context = TestDb.NewContext();
        var service = new CategoryService(context, TestDb.CreateMapper());

        var created = await service.CreateAsync(new CategoryInputDto { Name = "Cooling", Description = "Fans" });

        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(context.Categories.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_ModifiesCategory()
    {
        var service = BuildService(out var context);
        var ok = await service.UpdateAsync(new CategoryInputDto { Id = 1, Name = "GPUs", Description = "Updated" });

        Assert.That(ok, Is.True);
        Assert.That(context.Categories.Single(c => c.Id == 1).Name, Is.EqualTo("GPUs"));
    }

    [Test]
    public async Task UpdateAsync_ReturnsFalse_WhenMissing()
    {
        var service = BuildService(out _);
        var ok = await service.UpdateAsync(new CategoryInputDto { Id = 999, Name = "Nope" });

        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task GetForEditAsync_ReturnsInputDto()
    {
        var service = BuildService(out _);
        var input = await service.GetForEditAsync(1);

        Assert.That(input, Is.Not.Null);
        Assert.That(input!.Name, Is.EqualTo("Graphics Cards"));
    }

    [Test]
    public async Task DeleteAsync_RemovesCategory()
    {
        var service = BuildService(out var context);
        var ok = await service.DeleteAsync(2);

        Assert.That(ok, Is.True);
        Assert.That(context.Categories.Any(c => c.Id == 2), Is.False);
    }
}
