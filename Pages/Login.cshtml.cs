using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages;

public class LoginModel : PageModel
{
    private readonly ShopDbContext _context;
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public LoginModel(ShopDbContext context) => _context = context;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer?.PasswordHash == null || _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, Input.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new(ClaimTypes.Name, customer.FullName),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.Role, "User")
        };
        await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")),
            new AuthenticationProperties { IsPersistent = Input.RememberMe });

        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? Redirect(ReturnUrl)
            : RedirectToPage("/Index");
    }
}
