using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission("Customer.View")]
public class CustomerController(UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index() => View(users.Users.OrderBy(x => x.FullName).ToList());

    [HttpGet, HasPermission("Customer.Create")]
    public IActionResult Create() => View(new AdminCustomerFormViewModel());

    [HttpPost, ValidateAntiForgeryToken, HasPermission("Customer.Create")]
    public async Task<IActionResult> Create(AdminCustomerFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password)) ModelState.AddModelError(nameof(model.Password), "Mật khẩu là bắt buộc.");
        if (string.IsNullOrWhiteSpace(model.ConfirmPassword)) ModelState.AddModelError(nameof(model.ConfirmPassword), "Vui lòng xác nhận mật khẩu.");
        if (await users.FindByEmailAsync(model.Email) is not null) ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, PhoneNumber = model.Phone, IsActive = model.IsActive, EmailConfirmed = true };
        var result = await users.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(nameof(model.Email), e.Description);
            return View(model);
        }
        await users.AddToRoleAsync(user, "Customer");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission("Customer.Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await users.FindByIdAsync(id);
        return user is null ? NotFound() : View(new AdminCustomerFormViewModel { Id = user.Id, FullName = user.FullName, Email = user.Email!, Phone = user.PhoneNumber, IsActive = user.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission("Customer.Edit")]
    public async Task<IActionResult> Edit(string id, AdminCustomerFormViewModel model)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        ModelState.Remove(nameof(model.Email));
        if (!ModelState.IsValid) return View(model);
        user.FullName = model.FullName;
        user.PhoneNumber = model.Phone;
        user.IsActive = model.IsActive;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
        return result.Succeeded ? RedirectToAction(nameof(Index)) : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission("Customer.Lock")]
    public async Task<IActionResult> Toggle(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        user.IsActive = !user.IsActive;
        await users.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission("Customer.Delete")]
    public Task<IActionResult> Delete(string id) => Toggle(id);
}
