using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Controllers;

public class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("/account/login")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("/account/login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive) { ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng."); return View(model); }
        var result = await signInManager.PasswordSignInAsync(user, model.Password, false, true);
        if (!result.Succeeded) { ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng."); return View(model); }
        return Redirect(Url.IsLocalUrl(model.ReturnUrl) && !model.ReturnUrl!.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ? model.ReturnUrl : "/");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) { foreach (var error in result.Errors) ModelState.AddModelError(nameof(model.Email), error.Description); return View(model); }
        await userManager.AddToRoleAsync(user, "Customer");
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await signInManager.SignOutAsync(); return RedirectToAction("Index", "Home"); }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}