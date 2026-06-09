using NUnit.Framework;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Core.Querying;

namespace TechForge.Tests;

[TestFixture]
public class CoreModelsTests
{
    [Test]
    public void PagedResult_ComputesPagesAndNavigationFlags()
    {
        var page = new PagedResult<ProductDto>
        {
            Items = new[] { new ProductDto() },
            Page = 2,
            PageSize = 10,
            TotalCount = 35
        };

        Assert.That(page.TotalPages, Is.EqualTo(4)); // ceil(35/10)
        Assert.That(page.HasPrevious, Is.True);
        Assert.That(page.HasNext, Is.True);
    }

    [Test]
    public void PagedResult_FirstAndLastPage_FlagsAreCorrect()
    {
        var single = new PagedResult<ProductDto> { Page = 1, PageSize = 10, TotalCount = 5 };
        Assert.That(single.TotalPages, Is.EqualTo(1));
        Assert.That(single.HasPrevious, Is.False);
        Assert.That(single.HasNext, Is.False);
    }

    [Test]
    public void PagedResult_ZeroPageSize_DoesNotDivideByZero()
    {
        var p = new PagedResult<ProductDto> { PageSize = 0, TotalCount = 10 };
        Assert.That(p.TotalPages, Is.EqualTo(0));
    }

    [Test]
    public void CartDto_ComputesSubtotalItemsAndEmptiness()
    {
        var empty = new CartDto();
        Assert.That(empty.IsEmpty, Is.True);
        Assert.That(empty.Subtotal, Is.EqualTo(0m));

        var cart = new CartDto
        {
            Items =
            {
                new CartItemDto { UnitPrice = 100m, Quantity = 2 },
                new CartItemDto { UnitPrice = 50m, Quantity = 1 }
            }
        };

        Assert.That(cart.IsEmpty, Is.False);
        Assert.That(cart.TotalItems, Is.EqualTo(3));
        Assert.That(cart.Subtotal, Is.EqualTo(250m)); // 2*100 + 1*50
    }

    [Test]
    public void CartItemDto_LineTotal_IsPriceTimesQuantity()
    {
        var item = new CartItemDto { UnitPrice = 19.99m, Quantity = 3 };
        Assert.That(item.LineTotal, Is.EqualTo(59.97m));
    }

    [Test]
    public void OrderDto_ItemCount_SumsQuantities()
    {
        var order = new OrderDto
        {
            Items =
            {
                new OrderItemDto { Price = 10m, Quantity = 2 },
                new OrderItemDto { Price = 5m, Quantity = 4 }
            }
        };

        Assert.That(order.ItemCount, Is.EqualTo(6));
        Assert.That(order.Items[0].LineTotal, Is.EqualTo(20m));
    }

    [Test]
    public void ProductDto_InStock_ReflectsQuantity()
    {
        Assert.That(new ProductDto { StockQuantity = 0 }.InStock, Is.False);
        Assert.That(new ProductDto { StockQuantity = 3 }.InStock, Is.True);
    }

    [Test]
    public void ProductDetailsDto_And_WishlistItemDto_InStock()
    {
        Assert.That(new ProductDetailsDto { StockQuantity = 0 }.InStock, Is.False);
        Assert.That(new ProductDetailsDto { StockQuantity = 1 }.InStock, Is.True);
        Assert.That(new WishlistItemDto { StockQuantity = 0 }.InStock, Is.False);
        Assert.That(new WishlistItemDto { StockQuantity = 5 }.InStock, Is.True);
    }

    [Test]
    public void OrderItem_LineTotal_IsPriceTimesQuantity()
    {
        var item = new OrderItem { Price = 12.50m, Quantity = 4 };
        Assert.That(item.LineTotal, Is.EqualTo(50m));
    }

    [Test]
    public void ApplicationUser_FullName_CombinesNames()
    {
        var user = new ApplicationUser { FirstName = "Alex", LastName = "Rivera" };
        Assert.That(user.FullName, Is.EqualTo("Alex Rivera"));
    }
}
