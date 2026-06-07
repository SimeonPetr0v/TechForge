using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Entities;
using TechForge.Services.Contracts;

namespace TechForge.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviews;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(IReviewService reviews, UserManager<ApplicationUser> userManager)
    {
        _reviews = reviews;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, int rating, string comment)
    {
        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
        {
            TempData["ReviewError"] = "Please choose a rating (1–5 stars) and write a comment.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        var userId = _userManager.GetUserId(User)!;

        if (await _reviews.HasUserReviewedAsync(productId, userId))
        {
            TempData["ReviewError"] = "You have already reviewed this product.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        await _reviews.AddAsync(productId, userId, rating, comment.Trim());
        TempData["ReviewSuccess"] = "Thanks for your review!";
        return RedirectToAction("Details", "Products", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int reviewId, int productId)
    {
        var userId = _userManager.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");

        await _reviews.DeleteAsync(reviewId, userId, isAdmin);
        return RedirectToAction("Details", "Products", new { id = productId });
    }
}
