using System.ComponentModel.DataAnnotations;
using TechForge.Core.Dtos;

namespace TechForge.Web.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [StringLength(250, MinimumLength = 10)]
    [Display(Name = "Shipping address")]
    public string ShippingAddress { get; set; } = string.Empty;

    public CartDto Cart { get; set; } = new();
}
