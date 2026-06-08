using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Entities;
using TechForge.Services.Contracts;

namespace TechForge.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cart;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ICartService cart, UserManager<ApplicationUser> userManager)
    {
        _cart = cart;
        _userManager = userManager;
    }

    private string UserId => _userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _cart.GetCartAsync(UserId);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var added = await _cart.AddAsync(UserId, productId, quantity);
        var message = added ? "Added to your cart." : "Sorry, that item is unavailable.";

        if (IsAjaxRequest())
        {
            var count = await _cart.GetItemCountAsync(UserId);
            return Json(new { success = added, count, message });
        }

        TempData[added ? "FlashSuccess" : "FlashError"] = message;
        return RedirectToReferer();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
    {
        await _cart.UpdateQuantityAsync(UserId, productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await _cart.RemoveAsync(UserId, productId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await _cart.ClearAsync(UserId);
        return RedirectToAction(nameof(Index));
    }

    private bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

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
