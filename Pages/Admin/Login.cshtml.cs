using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace ShopUI.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var adminEmail = _configuration["Admin:Email"] ?? "admin@myshop.vn";
            var adminPassword = _configuration["Admin:Password"] ?? "Admin@123";
            if (!string.Equals(Input.Email, adminEmail, StringComparison.OrdinalIgnoreCase) || Input.Password != adminPassword)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu admin không chính xác.");
                return Page();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Quản trị viên"),
                new Claim(ClaimTypes.Email, adminEmail),
                new Claim(ClaimTypes.Role, "Admin")
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = Input.RememberMe });
            TempData["Msg"] = "Đăng nhập thành công.";
            return RedirectToPage("/Admin/Index");
        }
    }
}