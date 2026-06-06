using Microsoft.Extensions.DependencyInjection;
using TechForge.Services.Contracts;
using TechForge.Services.Implementations;
using TechForge.Services.Mapping;

namespace TechForge.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
