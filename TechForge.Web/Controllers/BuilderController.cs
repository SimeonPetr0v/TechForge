using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Entities;
using TechForge.Services.Contracts;

namespace TechForge.Web.Controllers;

public class BuilderController : Controller
{
    private readonly ICategoryService _categories;

    public BuilderController(ICategoryService categories)
    {
        _categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _categories.GetAllAsync();
        return View(categories);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBuild(
        int[] productIds,
        [FromServices] ICartService cart,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var selected = (productIds ?? Array.Empty<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (selected.Count == 0)
        {
            TempData["FlashError"] = "Select at least one component to build your PC.";
            return RedirectToAction(nameof(Index));
        }

        var userId = userManager.GetUserId(User)!;
        foreach (var id in selected)
        {
            await cart.AddAsync(userId, id, 1);
        }

        TempData["FlashSuccess"] = $"Added {selected.Count} component(s) from your build to the cart!";
        return RedirectToAction("Index", "Cart");
    }
}
