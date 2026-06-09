using NUnit.Framework;
using TechForge.Data;
using TechForge.Services.Implementations;

namespace TechForge.Tests;

[TestFixture]
public class ReviewServiceTests
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    private static ReviewService BuildService(out ApplicationDbContext context)
    {
        context = TestDb.NewContext();
        TestDb.SeedUser(context, UserId);
        TestDb.SeedUser(context, OtherUserId);
        TestDb.SeedCategory(context);
        TestDb.SeedProduct(context, 1, "GPU", 1000m);
        return new ReviewService(context, TestDb.CreateMapper());
    }

    [Test]
    public async Task AddAsync_CreatesReview_AndRecalculatesRating()
    {
        var service = BuildService(out var context);

        await service.AddAsync(productId: 1, UserId, rating: 4, comment: "Great card");
        await service.AddAsync(productId: 1, OtherUserId, rating: 2, comment: "Runs hot");

        Assert.That(context.Reviews.Count(), Is.EqualTo(2));
        var product = context.Products.Single(p => p.Id == 1);
        Assert.That(product.Rating, Is.EqualTo(3.0)); // (4 + 2) / 2
    }

    [Test]
    public async Task AddAsync_ReturnsNull_ForInvalidRating()
    {
        var service = BuildService(out _);

        var result = await service.AddAsync(1, UserId, rating: 6, comment: "Invalid");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddAsync_ReturnsNull_WhenProductMissing()
    {
        var service = BuildService(out _);

        var result = await service.AddAsync(productId: 999, UserId, rating: 5, comment: "Nope");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddAsync_ReturnsMappedDto_WithAuthor()
    {
        var service = BuildService(out _);

        var dto = await service.AddAsync(1, UserId, 5, "Excellent");

        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Rating, Is.EqualTo(5));
        Assert.That(dto.AuthorName, Is.EqualTo("Test User"));
    }

    [Test]
    public async Task HasUserReviewedAsync_ReturnsTrue_AfterReviewing()
    {
        var service = BuildService(out _);
        await service.AddAsync(1, UserId, 5, "Nice");

        Assert.That(await service.HasUserReviewedAsync(1, UserId), Is.True);
        Assert.That(await service.HasUserReviewedAsync(1, OtherUserId), Is.False);
    }

    [Test]
    public async Task DeleteAsync_AllowsOwner()
    {
        var service = BuildService(out var context);
        var review = await service.AddAsync(1, UserId, 5, "Mine");

        var ok = await service.DeleteAsync(review!.Id, UserId, isAdmin: false);

        Assert.That(ok, Is.True);
        Assert.That(context.Reviews.Any(), Is.False);
    }

    [Test]
    public async Task DeleteAsync_AllowsAdmin_ForAnyReview()
    {
        var service = BuildService(out var context);
        var review = await service.AddAsync(1, UserId, 5, "By user 1");

        var ok = await service.DeleteAsync(review!.Id, OtherUserId, isAdmin: true);

        Assert.That(ok, Is.True);
        Assert.That(context.Reviews.Any(), Is.False);
    }

    [Test]
    public async Task DeleteAsync_DeniesOtherNonAdminUser()
    {
        var service = BuildService(out var context);
        var review = await service.AddAsync(1, UserId, 5, "By user 1");

        var ok = await service.DeleteAsync(review!.Id, OtherUserId, isAdmin: false);

        Assert.That(ok, Is.False);
        Assert.That(context.Reviews.Count(), Is.EqualTo(1));
    }
}
