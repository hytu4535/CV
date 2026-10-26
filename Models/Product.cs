using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ShopMvc.Models;
public class Product
{
    public int Id { get; set; }
    [Required(ErrorMessage = "SKU là bắt buộc.")]
    [RegularExpression("^[A-Z0-9\\-]{3,30}$", ErrorMessage = "SKU chỉ gồm chữ in hoa, số và dấu gạch ngang, từ 3 đến 30 ký tự.")]
    public string Sku { get; set; } = string.Empty;
    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3 đến 150 ký tự.")]
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1.000 đến 1.000.000.000.")]
    [Column(TypeName = "decimal(18,0)")] public decimal Price { get; set; }
    [Column(TypeName = "decimal(18,0)")] public decimal? SalePrice { get; set; }
    [Range(0, 100000, ErrorMessage = "Tồn kho phải từ 0 đến 100.000.")] public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    [NotMapped] public decimal CurrentPrice => SalePrice ?? Price;
}
