using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechForge.Services.Contracts;

namespace TechForge.Web.ViewComponents;

public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cart;

    public CartSummaryViewComponent(ICartService cart)
    {
        _cart = cart;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.Identity?.IsAuthenticated != true)
        {
            return View(0);
        }

        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var count = userId is null ? 0 : await _cart.GetItemCountAsync(userId);
        return View(count);
    }
}
