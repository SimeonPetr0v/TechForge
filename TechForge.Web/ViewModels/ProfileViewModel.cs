using System.ComponentModel.DataAnnotations;

namespace TechForge.Web.ViewModels;

public class ProfileViewModel
{
    [Required]
    [StringLength(80)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(500)]
    [Url]
    [Display(Name = "Profile image URL")]
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Member since")]
    public DateTime CreatedOn { get; set; }
}
