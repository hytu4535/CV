using System.ComponentModel.DataAnnotations;
namespace ShopMvc.ViewModels;
public class LoginViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}

public sealed class AdminProductFormViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3 đến 150 ký tự.")]
    [Display(Name = "Tên sản phẩm")]
    public string Name { get; set; } = string.Empty;
    [Range(1000, 1000000000, ErrorMessage = "Giá bán phải từ 1.000 đến 1.000.000.000 đồng.")]
    [Display(Name = "Giá bán")]
    public decimal Price { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }
    [Range(0, 100000, ErrorMessage = "Tồn kho phải từ 0 đến 100.000.")]
    [Display(Name = "Tồn kho")]
    public int Stock { get; set; }
    [Display(Name = "Ghi chú")]
    public string? Description { get; set; }
}
public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = string.Empty;
    [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d).{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, chữ hoa, chữ thường và số.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
public class ProfileViewModel
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [RegularExpression("^[\\p{L}]+(?:[ \\t]+[\\p{L}]+)*$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;
    [Display(Name = "Số điện thoại")]
    [RegularExpression("^(0(3|5|7|8|9)\\d{8}|\\+84(3|5|7|8|9)\\d{8})$", ErrorMessage = "Số điện thoại phải có dạng 0xxxxxxxxx hoặc +84xxxxxxxxx.")]
    public string? Phone { get; set; }
}
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [RegularExpression("^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d).{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, chữ hoa, chữ thường và số.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
