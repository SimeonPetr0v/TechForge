using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Entities;
using TechForge.Services.Contracts;

namespace TechForge.Web.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlist;
    private readonly UserManager<ApplicationUser> _userManager;

    public WishlistController(IWishlistService wishlist, UserManager<ApplicationUser> userManager)
    {
        _wishlist = wishlist;
        _userManager = userManager;
    }

    private string UserId => _userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _wishlist.GetWishlistAsync(UserId);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId)
    {
        var nowInWishlist = await _wishlist.ToggleAsync(UserId, productId);
        TempData["FlashSuccess"] = nowInWishlist
            ? "Added to your wishlist."
            : "Removed from your wishlist.";

        return RedirectToReferer();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _wishlist.RemoveAsync(UserId, productId);
        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToReferer()
    {
        var referer = Request.Headers["Referer"].ToString();
        if (Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            && string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(referer);
        }

        return RedirectToAction(nameof(Index));
    }
}
