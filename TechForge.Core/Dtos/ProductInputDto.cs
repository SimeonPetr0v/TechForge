using System.ComponentModel.DataAnnotations;

namespace TechForge.Core.Dtos;

public class ProductInputDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Brand { get; set; } = string.Empty;

    [Range(0.01, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock quantity")]
    public int StockQuantity { get; set; }

    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [StringLength(4000)]
    public string? Specifications { get; set; }

    [Display(Name = "Featured")]
    public bool IsFeatured { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Release date")]
    public DateTime ReleaseDate { get; set; }
}
