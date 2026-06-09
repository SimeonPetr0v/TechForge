using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechForge.Core.Entities;
using TechForge.Data;
using TechForge.Services.Mapping;

namespace TechForge.Tests;

/// <summary>
/// Test helpers: a fresh in-memory EF Core database per call, and a real
/// AutoMapper instance configured with the application's MappingProfile.
/// </summary>
public static class TestDb
{
    public static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    public static ApplicationUser SeedUser(ApplicationDbContext context, string id = "user-1")
    {
        var user = new ApplicationUser
        {
            Id = id,
            UserName = $"{id}@test.local",
            Email = $"{id}@test.local",
            FirstName = "Test",
            LastName = "User"
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    public static Category SeedCategory(ApplicationDbContext context, int id = 1, string name = "Graphics Cards")
    {
        var category = new Category { Id = id, Name = name, Description = name };
        context.Categories.Add(category);
        context.SaveChanges();
        return category;
    }

    public static Product SeedProduct(
        ApplicationDbContext context,
        int id,
        string name,
        decimal price,
        int stock = 10,
        int categoryId = 1,
        bool featured = false,
        string brand = "TestBrand")
    {
        var product = new Product
        {
            Id = id,
            Name = name,
            Description = $"{name} description text",
            Brand = brand,
            Price = price,
            StockQuantity = stock,
            CategoryId = categoryId,
            IsFeatured = featured,
            ReleaseDate = new DateTime(2024, 1, 1),
            CreatedOn = new DateTime(2024, 1, id)
        };
        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }
}
