using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopMvc.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    [Required, StringLength(150)] public string ProductName { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,0)")] public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}