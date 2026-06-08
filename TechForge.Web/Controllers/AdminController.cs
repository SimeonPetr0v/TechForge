using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;
using TechForge.Core.Enums;
using TechForge.Core.Querying;
using TechForge.Services.Contracts;
using TechForge.Web.ViewModels;

namespace TechForge.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;
    private readonly IOrderService _orders;
    private readonly IReviewService _reviews;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        IProductService products,
        ICategoryService categories,
        IOrderService orders,
        IReviewService reviews,
        UserManager<ApplicationUser> userManager)
    {
        _products = products;
        _categories = categories;
        _orders = orders;
        _reviews = reviews;
        _userManager = userManager;
    }

    // ---------- Dashboard ----------

    public async Task<IActionResult> Index()
    {
        var products = await _products.GetCatalogAsync(new ProductQueryOptions { PageSize = 1000 });
        var orders = await _orders.GetAllAsync();
        var categories = await _categories.GetAllAsync();

        var model = new DashboardViewModel
        {
            ProductCount = products.TotalCount,
            CategoryCount = categories.Count,
            OrderCount = orders.Count,
            UserCount = _userManager.Users.Count(),
            TotalRevenue = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalPrice),
            LowStock = products.Items.Where(p => p.StockQuantity <= 5).OrderBy(p => p.StockQuantity).Take(5).ToList(),
            RecentOrders = orders.Take(5).ToList()
        };

        return View(model);
    }

    // ---------- Products ----------

    public async Task<IActionResult> Products()
    {
        var products = await _products.GetCatalogAsync(new ProductQueryOptions
        {
            PageSize = 1000,
            Sort = ProductSortOption.NameAToZ
        });
        return View(products.Items);
    }

    [HttpGet]
    public async Task<IActionResult> ProductCreate()
    {
        await PopulateCategoriesAsync();
        return View("ProductForm", new ProductInputDto { ReleaseDate = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCreate(ProductInputDto input)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View("ProductForm", input);
        }

        await _products.CreateAsync(input);
        TempData["FlashSuccess"] = "Product created.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> ProductEdit(int id)
    {
        var input = await _products.GetForEditAsync(id);
        if (input is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync();
        return View("ProductForm", input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductEdit(ProductInputDto input)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View("ProductForm", input);
        }

        var updated = await _products.UpdateAsync(input);
        if (!updated)
        {
            return NotFound();
        }

        TempData["FlashSuccess"] = "Product updated.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductDelete(int id)
    {
        try
        {
            await _products.DeleteAsync(id);
            TempData["FlashSuccess"] = "Product deleted.";
        }
        catch
        {
            TempData["FlashError"] = "This product can't be deleted because it belongs to existing orders.";
        }

        return RedirectToAction(nameof(Products));
    }

    // ---------- Categories ----------

    public async Task<IActionResult> Categories()
    {
        var categories = await _categories.GetAllAsync();
        return View(categories);
    }

    [HttpGet]
    public IActionResult CategoryCreate() => View("CategoryForm", new CategoryInputDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryCreate(CategoryInputDto input)
    {
        if (!ModelState.IsValid)
        {
            return View("CategoryForm", input);
        }

        await _categories.CreateAsync(input);
        TempData["FlashSuccess"] = "Category created.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> CategoryEdit(int id)
    {
        var input = await _categories.GetForEditAsync(id);
        if (input is null)
        {
            return NotFound();
        }

        return View("CategoryForm", input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryEdit(CategoryInputDto input)
    {
        if (!ModelState.IsValid)
        {
            return View("CategoryForm", input);
        }

        var updated = await _categories.UpdateAsync(input);
        if (!updated)
        {
            return NotFound();
        }

        TempData["FlashSuccess"] = "Category updated.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CategoryDelete(int id)
    {
        try
        {
            await _categories.DeleteAsync(id);
            TempData["FlashSuccess"] = "Category deleted.";
        }
        catch
        {
            TempData["FlashError"] = "This category can't be deleted while it still has products.";
        }

        return RedirectToAction(nameof(Categories));
    }

    // ---------- Orders ----------

    public async Task<IActionResult> Orders()
    {
        var orders = await _orders.GetAllAsync();
        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderStatus(int id, OrderStatus status)
    {
        await _orders.UpdateStatusAsync(id, status);
        TempData["FlashSuccess"] = $"Order #{id} marked as {status}.";
        return RedirectToAction(nameof(Orders));
    }

    // ---------- Users ----------

    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.OrderBy(u => u.Email).ToList();
        var model = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new AdminUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = string.Join(", ", roles),
                CreatedOn = user.CreatedOn
            });
        }

        return View(model);
    }

    // ---------- Reviews ----------

    public async Task<IActionResult> Reviews()
    {
        var reviews = await _reviews.GetAllAsync();
        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewDelete(int id)
    {
        var adminId = _userManager.GetUserId(User)!;
        await _reviews.DeleteAsync(id, adminId, isAdmin: true);
        TempData["FlashSuccess"] = "Review deleted.";
        return RedirectToAction(nameof(Reviews));
    }

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.Categories = await _categories.GetAllAsync();
    }
}
