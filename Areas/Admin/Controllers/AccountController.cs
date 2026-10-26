using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopMvc.Models;
using ShopMvc.ViewModels;
namespace ShopMvc.Areas.Admin.Controllers;
[Area("Admin")]
public class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("/admin/login")] public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });
    [HttpPost("/admin/login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var admin = await userManager.FindByEmailAsync(model.Email);
        if (admin is null || !admin.IsActive || !await userManager.IsInRoleAsync(admin, "Admin")) { ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng."); return View(model); }
        var result = await signInManager.PasswordSignInAsync(admin, model.Password, false, true);
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng."); return View(model); }
        return Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl!.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ? Redirect(model.ReturnUrl) : Redirect("/admin");
    }
    [HttpPost("/admin/logout"), ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await signInManager.SignOutAsync(); return Redirect("/admin/login"); }
    public IActionResult AccessDenied() => View();
}
