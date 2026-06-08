using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Entities;
using TechForge.Services.Contracts;
using TechForge.Web.ViewModels;

namespace TechForge.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;
    private readonly ICartService _cart;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(
        IOrderService orders,
        ICartService cart,
        UserManager<ApplicationUser> userManager)
    {
        _orders = orders;
        _cart = cart;
        _userManager = userManager;
    }

    private string UserId => _userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = await _cart.GetCartAsync(UserId);
        if (cart.IsEmpty)
        {
            TempData["FlashError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userManager.GetUserAsync(User);

        var model = new CheckoutViewModel
        {
            Cart = cart,
            ShippingAddress = user?.Address ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = await _cart.GetCartAsync(UserId);
        if (cart.IsEmpty)
        {
            TempData["FlashError"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            model.Cart = cart;
            return View(model);
        }

        var order = await _orders.PlaceOrderAsync(UserId, model.ShippingAddress.Trim());
        if (order is null)
        {
            TempData["FlashError"] = "We couldn't place your order. Please try again.";
            return RedirectToAction("Index", "Cart");
        }

        TempData["FlashSuccess"] = $"Order #{order.Id} placed successfully!";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orders.GetUserOrdersAsync(UserId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetByIdAsync(id, UserId);
        if (order is null)
        {
            return NotFound();
        }

        return View(order);
    }
}
