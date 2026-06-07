using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Querying;
using TechForge.Services.Contracts;
using TechForge.Web.ViewModels;

namespace TechForge.Web.Controllers;

public class ProductsController : Controller
{
    private const int PageSize = 9;

    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    public ProductsController(IProductService products, ICategoryService categories)
    {
        _products = products;
        _categories = categories;
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
}
