using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopMvc.Models;

public enum OrderStatus { Pending, Shipping, Completed, Cancelled }

public class Order
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string Code { get; set; } = string.Empty;
    [Required] public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    [Required, StringLength(100)] public string ReceiverName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string Phone { get; set; } = string.Empty;
    [Required, StringLength(250)] public string Address { get; set; } = string.Empty;
    [StringLength(500)] public string? Note { get; set; }
    [Required, StringLength(30)] public string PaymentMethod { get; set; } = "COD";
    [Column(TypeName = "decimal(18,0)")] public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}