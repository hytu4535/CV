using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ShopUI.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,100}$", ErrorMessage = "Họ tên chỉ gồm chữ cái, 2-100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[\w\.\-]+@[\w\-]+(\.[\w\-]+)+$", ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0|\+84)(3|5|7|8|9)\d{8}$", ErrorMessage = "Số điện thoại VN không hợp lệ (VD: 0912345678)")]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(255)]
        public string? PasswordHash { get; set; }
    }
}