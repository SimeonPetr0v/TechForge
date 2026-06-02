using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TechForge.Core.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
