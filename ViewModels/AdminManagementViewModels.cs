using System.ComponentModel.DataAnnotations;
using ShopMvc.Models;

namespace ShopMvc.ViewModels;

public sealed class AdminCustomerFormViewModel
{
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự.")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }
    [StringLength(250, ErrorMessage = "Địa chỉ không được vượt quá 250 ký tự.")]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
    [Display(Name = "Mật khẩu")]
    public string? Password { get; set; }
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string? ConfirmPassword { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class AdminUserFormViewModel
{
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự.")]
    [Display(Name = "Họ tên")] public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
    [Display(Name = "Mật khẩu mới")] public string? Password { get; set; }
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu mới")] public string? ConfirmPassword { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
    [Display(Name = "Vai trò")] public string RoleId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class RoleFormViewModel
{
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập tên vai trò."), StringLength(100, MinimumLength = 2, ErrorMessage = "Tên vai trò phải từ 2 đến 100 ký tự.")] public string Name { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public string[] PermissionIds { get; set; } = [];
    public IReadOnlyList<PermissionOptionViewModel> Permissions { get; set; } = [];
}

public sealed class PermissionOptionViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public bool Selected { get; init; }
}

public sealed class DashboardViewModel
{
    public int ProductCount { get; init; }
    public int CategoryCount { get; init; }
    public int OrderCount { get; init; }
    public int AccountCount { get; init; }
    public IReadOnlyList<Order> LatestOrders { get; init; } = [];
}

public sealed class PagedResultViewModel<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public sealed class AdminUserListItemViewModel
{
    public ApplicationUser User { get; init; } = null!;
    public IReadOnlyList<string> Roles { get; init; } = [];
}

public sealed class ResetPasswordViewModel
{
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
