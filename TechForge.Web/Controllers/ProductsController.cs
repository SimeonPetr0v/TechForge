using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Entities;
using TechForge.Core.Querying;
using TechForge.Services.Contracts;
using TechForge.Web.ViewModels;

namespace TechForge.Web.Controllers;

public class ProductsController : Controller
{
    private const int PageSize = 9;

    private readonly IProductService _products;
    private readonly ICategoryService _categories;
    private readonly IReviewService _reviews;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductsController(
        IProductService products,
        ICategoryService categories,
        IReviewService reviews,
        UserManager<ApplicationUser> userManager)
    {
        _products = products;
        _categories = categories;
        _reviews = reviews;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CatalogViewModel filter)
    {
        var options = new ProductQueryOptions
        {
            SearchTerm = filter.Search,
            CategoryId = filter.CategoryId,
            Brand = filter.Brand,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            InStockOnly = filter.InStockOnly,
            Sort = filter.Sort,
            Page = filter.Page < 1 ? 1 : filter.Page,
            PageSize = PageSize
        };

        filter.Products = await _products.GetCatalogAsync(options);
        filter.Categories = await _categories.GetAllAsync();
        filter.Brands = await _products.GetBrandsAsync();

        return View(filter);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _products.GetDetailsAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var currentUserId = isAuthenticated ? _userManager.GetUserId(User) : null;

        var vm = new ProductDetailsViewModel
        {
            Product = product,
            Related = await _products.GetRelatedAsync(id, 4),
            IsAuthenticated = isAuthenticated,
            IsAdmin = User.IsInRole("Admin"),
            CurrentUserId = currentUserId,
            HasReviewed = currentUserId is not null
                && await _reviews.HasUserReviewedAsync(id, currentUserId)
        };

        return View(vm);
    }
}
