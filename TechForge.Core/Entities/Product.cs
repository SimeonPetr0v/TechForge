using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechForge.Core.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Brand { get; set; } = string.Empty;

    [Range(0.01, 1_000_000)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; }

    [StringLength(4000)]
    public string? Specifications { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime ReleaseDate { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
