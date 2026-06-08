using System.ComponentModel.DataAnnotations;

namespace TechForge.Core.Dtos;

public class CategoryInputDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}
