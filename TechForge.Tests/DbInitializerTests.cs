using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechForge.Core.Entities;
using TechForge.Data;
using TechForge.Data.Seeding;

namespace TechForge.Tests;

[TestFixture]
public class DbInitializerTests
{
    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AdminUser:Email"] = "admin@techforge.local",
            ["AdminUser:Password"] = "Admin#12345",
            ["AdminUser:FirstName"] = "Admin",
            ["AdminUser:LastName"] = "TechForge"
        })
        .Build();

    private static IServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o =>
            o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task SeedAsync_SeedsCategoriesProductsAndReviews()
    {
        var sp = BuildProvider();
        var ctx = sp.GetRequiredService<ApplicationDbContext>();

        await DbInitializer.SeedAsync(
            ctx,
            sp.GetRequiredService<UserManager<ApplicationUser>>(),
            sp.GetRequiredService<RoleManager<IdentityRole>>(),
            BuildConfig());

        Assert.That(ctx.Categories.Count(), Is.EqualTo(8));
        Assert.That(ctx.Products.Count(), Is.EqualTo(12));
        Assert.That(ctx.Reviews.Any(), Is.True);
    }

    [Test]
    public async Task SeedAsync_CreatesRolesAndAdminInAdminRole()
    {
        var sp = BuildProvider();
        var ctx = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.SeedAsync(ctx, userManager, roleManager, BuildConfig());

        Assert.That(await roleManager.RoleExistsAsync(DbInitializer.AdminRole), Is.True);
        Assert.That(await roleManager.RoleExistsAsync(DbInitializer.CustomerRole), Is.True);

        var admin = await userManager.FindByEmailAsync("admin@techforge.local");
        Assert.That(admin, Is.Not.Null);
        Assert.That(await userManager.IsInRoleAsync(admin!, DbInitializer.AdminRole), Is.True);
    }

    [Test]
    public async Task SeedAsync_IsIdempotent()
    {
        var sp = BuildProvider();
        var ctx = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.SeedAsync(ctx, userManager, roleManager, BuildConfig());
        await DbInitializer.SeedAsync(ctx, userManager, roleManager, BuildConfig());

        Assert.That(ctx.Categories.Count(), Is.EqualTo(8));
        Assert.That(ctx.Products.Count(), Is.EqualTo(12));
    }
}
