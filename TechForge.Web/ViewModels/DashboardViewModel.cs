using TechForge.Core.Dtos;

namespace TechForge.Web.ViewModels;

public class DashboardViewModel
{
    public int ProductCount { get; set; }

    public int CategoryCount { get; set; }

    public int OrderCount { get; set; }

    public int UserCount { get; set; }

    public decimal TotalRevenue { get; set; }

    public IReadOnlyList<ProductDto> LowStock { get; set; } = Array.Empty<ProductDto>();

    public IReadOnlyList<OrderDto> RecentOrders { get; set; } = Array.Empty<OrderDto>();
}
