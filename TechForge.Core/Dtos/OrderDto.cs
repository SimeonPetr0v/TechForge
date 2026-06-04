namespace TechForge.Core.Dtos;

public class OrderDto
{
    public int Id { get; set; }

    public DateTime OrderedOn { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public string? ShippingAddress { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();

    public int ItemCount => Items.Sum(i => i.Quantity);
}
