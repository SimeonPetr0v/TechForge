using System.ComponentModel.DataAnnotations;

namespace TechForge.Core.Entities;

public class CartItem
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; } = 1;

    public DateTime AddedOn { get; set; } = DateTime.UtcNow;
}
