using Microsoft.AspNetCore.Mvc;
using TechForge.Core.Querying;
using TechForge.Services.Contracts;

namespace TechForge.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductApiController : ControllerBase
{
    private readonly IProductService _products;

    public ProductApiController(IProductService products)
    {
        _products = products;
    }

    // GET /api/products?search=&categoryId=&sort=&page=
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ProductQueryOptions options)
    {
        var page = await _products.GetCatalogAsync(options);
        return Ok(page);
    }

    // GET /api/products/search?term=rtx&limit=6
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? term, [FromQuery] int limit = 6)
    {
        var results = await _products.SearchAsync(term ?? string.Empty, limit);
        return Ok(results);
    }

    // GET /api/products/by-category/3
    [HttpGet("by-category/{categoryId:int}")]
    public async Task<IActionResult> ByCategory(int categoryId)
    {
        var page = await _products.GetCatalogAsync(new ProductQueryOptions
        {
            CategoryId = categoryId,
            Sort = ProductSortOption.PriceLowToHigh,
            PageSize = 100
        });

        return Ok(page.Items);
    }

    // GET /api/products/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _products.GetDetailsAsync(id);
        return product is null ? NotFound() : Ok(product);
    }
}
