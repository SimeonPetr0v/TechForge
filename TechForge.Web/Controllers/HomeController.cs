using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Querying;
using TechForge.Services.Contracts;
using TechForge.Web.ViewModels;

namespace TechForge.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    public HomeController(IProductService products, ICategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<IActionResult> Index()
    {
        var newest = await _products.GetCatalogAsync(new ProductQueryOptions
        {
            Sort = ProductSortOption.Newest,
            PageSize = 4
        });

        var model = new HomeViewModel
        {
            Featured = await _products.GetFeaturedAsync(4),
            Newest = newest.Items,
            Categories = await _categories.GetAllAsync()
        };

        return View(model);
    }

    public IActionResult Privacy() => View();
}
