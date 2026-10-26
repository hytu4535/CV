using System.ComponentModel.DataAnnotations;

namespace ShopMvc.Models;

public class Customer
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    public string? Phone { get; set; }
    [StringLength(250)]
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
