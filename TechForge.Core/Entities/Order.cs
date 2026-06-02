using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechForge.Core.Enums;

namespace TechForge.Core.Entities;

public class Order
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime OrderedOn { get; set; } = DateTime.UtcNow;

    [StringLength(250)]
    public string? ShippingAddress { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
