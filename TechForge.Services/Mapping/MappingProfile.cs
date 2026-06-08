using AutoMapper;
using TechForge.Core.Dtos;
using TechForge.Core.Entities;

namespace TechForge.Services.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty));

        CreateMap<Product, ProductDetailsDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Reviews.Count))
            .ForMember(d => d.Reviews,
                o => o.MapFrom(s => s.Reviews.OrderByDescending(r => r.CreatedOn)));

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.AuthorId, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.AuthorName,
                o => o.MapFrom(s => s.User != null ? s.User.FirstName + " " + s.User.LastName : "Anonymous"));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));

        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product != null ? s.Product.ImageUrl : null))
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Product != null ? s.Product.Price : 0))
            .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Product != null ? s.Product.StockQuantity : 0));

        CreateMap<WishlistItem, WishlistItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.Brand, o => o.MapFrom(s => s.Product != null ? s.Product.Brand : string.Empty))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Product != null ? s.Product.Price : 0))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product != null ? s.Product.ImageUrl : null))
            .ForMember(d => d.Rating, o => o.MapFrom(s => s.Product != null ? s.Product.Rating : 0))
            .ForMember(d => d.StockQuantity, o => o.MapFrom(s => s.Product != null ? s.Product.StockQuantity : 0));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.Product != null ? s.Product.ImageUrl : null));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<ProductInputDto, Product>()
            .ForMember(d => d.CreatedOn, o => o.Ignore())
            .ForMember(d => d.Rating, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore());

        CreateMap<Product, ProductInputDto>();

        CreateMap<CategoryInputDto, Category>()
            .ForMember(d => d.Products, o => o.Ignore());

        CreateMap<Category, CategoryInputDto>();
    }
}
