namespace TechForge.Core.Dtos;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();

    public decimal Subtotal => Items.Sum(i => i.LineTotal);

    public int TotalItems => Items.Sum(i => i.Quantity);

    public bool IsEmpty => Items.Count == 0;
}
