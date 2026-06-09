using AutoMapper;
using Moq;
using NUnit.Framework;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

/// <summary>
/// Demonstrates isolating the service from AutoMapper using Moq:
/// the in-memory DbContext provides real query results, while a mocked
/// IMapper lets us verify exactly what the service hands to the mapper.
/// </summary>
[TestFixture]
public class ProductServiceMockTests
{
    [Test]
    public async Task GetFeaturedAsync_PassesOnlyFeaturedProductsToMapper()
    {
        var context = TestDb.NewContext();
        TestDb.SeedCategory(context);
        TestDb.SeedProduct(context, 1, "Featured GPU", 1000m, featured: true);
        TestDb.SeedProduct(context, 2, "Regular GPU", 900m, featured: false);

        var expected = new List<ProductDto> { new() { Id = 1, Name = "Featured GPU" } };

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<List<ProductDto>>(It.IsAny<object>()))
            .Returns(expected);

        var service = new ProductService(context, mapperMock.Object);

        var result = await service.GetFeaturedAsync(10);

        // Service returns exactly what the (mocked) mapper produced.
        Assert.That(result, Is.SameAs(expected));

        // And it only handed the featured product to the mapper.
        mapperMock.Verify(
            m => m.Map<List<ProductDto>>(It.Is<List<Product>>(list =>
                list.Count == 1 && list[0].Id == 1)),
            Times.Once);
    }

    [Test]
    public async Task GetForEditAsync_ReturnsNull_WithoutCallingMapper_WhenMissing()
    {
        var context = TestDb.NewContext();
        var mapperMock = new Mock<IMapper>();
        var service = new ProductService(context, mapperMock.Object);

        var result = await service.GetForEditAsync(404);

        Assert.That(result, Is.Null);
        mapperMock.Verify(m => m.Map<ProductInputDto>(It.IsAny<object>()), Times.Never);
    }
}
