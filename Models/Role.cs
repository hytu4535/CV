using System.ComponentModel.DataAnnotations;
namespace ShopMvc.Models;
public class Role
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Tên vai trò là bắt buộc.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên vai trò phải từ 2 đến 100 ký tự.")]
    public string Name { get; set; } = string.Empty;
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
