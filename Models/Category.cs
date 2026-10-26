using System.ComponentModel.DataAnnotations;
namespace ShopMvc.Models;
public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2 đến 100 ký tự.")]
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
