using NUnit.Framework;
using TechForge.Core.Dtos;
using TechForge.Core.Querying;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

[TestFixture]
public class ProductServiceTests
{
    private static ProductService BuildService(out TechForge.Data.ApplicationDbContext context)
    {
        context = TestDb.NewContext();
        TestDb.SeedCategory(context, 1, "Graphics Cards");
        TestDb.SeedCategory(context, 2, "Processors");
        TestDb.SeedProduct(context, 1, "ForgeVision RTX 4080", 1200m, stock: 5, categoryId: 1, featured: true, brand: "NVIDIA");
        TestDb.SeedProduct(context, 2, "Radeon RX 7900", 950m, stock: 0, categoryId: 1, featured: false, brand: "AMD");
        TestDb.SeedProduct(context, 3, "Core i9", 560m, stock: 8, categoryId: 2, featured: true, brand: "Intel");
        TestDb.SeedProduct(context, 4, "Ryzen 9", 540m, stock: 3, categoryId: 2, featured: false, brand: "AMD");
        return new ProductService(context, TestDb.CreateMapper());
    }

    [Test]
    public async Task GetCatalogAsync_ReturnsAllProducts_WithTotalCount()
    {
        var service = BuildService(out _);
        var result = await service.GetCatalogAsync(new ProductQueryOptions { PageSize = 10 });

        Assert.That(result.TotalCount, Is.EqualTo(4));
        Assert.That(result.Items, Has.Count.EqualTo(4));
    }

    [Test]
    public async Task GetCatalogAsync_FiltersBySearchTerm()
    {
        var service = BuildService(out _);
        var result = await service.GetCatalogAsync(new ProductQueryOptions { SearchTerm = "ryzen" });

        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items[0].Name, Is.EqualTo("Ryzen 9"));
    }

    [Test]
    public async Task GetCatalogAsync_FiltersByCategory()
    {
        var service = BuildService(out _);
        var result = await service.GetCatalogAsync(new ProductQueryOptions { CategoryId = 2 });

        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.All(p => p.CategoryName == "Processors"), Is.True);
    }

    [Test]
    public async Task GetCatalogAsync_InStockOnly_ExcludesOutOfStock()
    {
        var service = BuildService(out _);
        var result = await service.GetCatalogAsync(new ProductQueryOptions { InStockOnly = true });

        Assert.That(result.Items.Any(p => p.Name == "Radeon RX 7900"), Is.False);
        Assert.That(result.Items.All(p => p.StockQuantity > 0), Is.True);
    }

    [Test]
    public async Task GetCatalogAsync_SortsByPriceLowToHigh()
    {
        var service = BuildService(out _);
        var result = await service.GetCatalogAsync(new ProductQueryOptions { Sort = ProductSortOption.PriceLowToHigh });

        var prices = result.Items.Select(p => p.Price).ToList();
        Assert.That(prices, Is.Ordered.Ascending);
    }

    [Test]
    public async Task GetCatalogAsync_PaginatesResults()
    {
        var service = BuildService(out _);
        var page1 = await service.GetCatalogAsync(new ProductQueryOptions { Page = 1, PageSize = 2 });

        Assert.That(page1.Items, Has.Count.EqualTo(2));
        Assert.That(page1.TotalCount, Is.EqualTo(4));
        Assert.That(page1.TotalPages, Is.EqualTo(2));
        Assert.That(page1.HasNext, Is.True);
        Assert.That(page1.HasPrevious, Is.False);
    }

    [Test]
    public async Task GetFeaturedAsync_ReturnsOnlyFeatured()
    {
        var service = BuildService(out _);
        var featured = await service.GetFeaturedAsync(10);

        Assert.That(featured, Has.Count.EqualTo(2));
        Assert.That(featured.All(p => p.IsFeatured), Is.True);
    }

    [Test]
    public async Task GetDetailsAsync_ReturnsProduct_WhenExists()
    {
        var service = BuildService(out _);
        var details = await service.GetDetailsAsync(1);

        Assert.That(details, Is.Not.Null);
        Assert.That(details!.Name, Is.EqualTo("ForgeVision RTX 4080"));
        Assert.That(details.CategoryName, Is.EqualTo("Graphics Cards"));
    }

    [Test]
    public async Task GetDetailsAsync_ReturnsNull_WhenMissing()
    {
        var service = BuildService(out _);
        var details = await service.GetDetailsAsync(999);

        Assert.That(details, Is.Null);
    }

    [Test]
    public async Task GetBrandsAsync_ReturnsDistinctSortedBrands()
    {
        var service = BuildService(out _);
        var brands = await service.GetBrandsAsync();

        Assert.That(brands, Is.EqualTo(new[] { "AMD", "Intel", "NVIDIA" }));
    }

    [Test]
    public async Task CreateAsync_AddsProduct()
    {
        var context = TestDb.NewContext();
        TestDb.SeedCategory(context, 1, "Graphics Cards");
        var service = new ProductService(context, TestDb.CreateMapper());

        var dto = new ProductInputDto
        {
            Name = "New GPU",
            Description = "A brand new GPU",
            Brand = "NVIDIA",
            Price = 700m,
            StockQuantity = 10,
            CategoryId = 1,
            ReleaseDate = DateTime.UtcNow
        };

        var created = await service.CreateAsync(dto);

        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(context.Products.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_ModifiesProduct()
    {
        var service = BuildService(out var context);
        var input = await service.GetForEditAsync(1);
        input!.Price = 999m;
        input.Name = "Updated Name";

        var ok = await service.UpdateAsync(input);

        Assert.That(ok, Is.True);
        var updated = context.Products.Single(p => p.Id == 1);
        Assert.That(updated.Price, Is.EqualTo(999m));
        Assert.That(updated.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task DeleteAsync_RemovesProduct()
    {
        var service = BuildService(out var context);
        var ok = await service.DeleteAsync(4);

        Assert.That(ok, Is.True);
        Assert.That(context.Products.Any(p => p.Id == 4), Is.False);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenMissing()
    {
        var service = BuildService(out _);
        var ok = await service.DeleteAsync(999);

        Assert.That(ok, Is.False);
    }
}
