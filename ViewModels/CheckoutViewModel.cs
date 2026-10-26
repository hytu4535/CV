using System.ComponentModel.DataAnnotations;

namespace ShopMvc.ViewModels;

public sealed class CheckoutViewModel
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(100)]
    [RegularExpression("^[\\p{L}]+(?:[ \\t]+[\\p{L}]+)*$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression("^(0(3|5|7|8|9)\\d{8}|\\+84(3|5|7|8|9)\\d{8})$", ErrorMessage = "Số điện thoại phải có dạng 0xxxxxxxxx hoặc +84xxxxxxxxx.")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
    [StringLength(250)]
    [RegularExpression("^[\\p{L}\\p{N}\\s,./#-]+$", ErrorMessage = "Địa chỉ chứa ký tự không hợp lệ.")]
    [Display(Name = "Địa chỉ nhận hàng")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }
}
