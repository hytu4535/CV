using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission("AdminUser.View")]
public class AdminUserController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles) : Controller
{
    public IActionResult Index() => View(users.Users.OrderBy(x => x.FullName).ToList());

    [HttpGet, HasPermission("AdminUser.Create")]
    public async Task<IActionResult> Create() { await LoadRoles(); return View(new AdminUserFormViewModel()); }

    [HttpPost, ValidateAntiForgeryToken, HasPermission("AdminUser.Create")]
    public async Task<IActionResult> Create(AdminUserFormViewModel model)
    {
        if (!ModelState.IsValid) { await LoadRoles(); return View(model); }
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, IsActive = model.IsActive, EmailConfirmed = true };
        var result = await users.CreateAsync(user, model.Password!);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError(nameof(model.Email), e.Description); await LoadRoles(); return View(model); }
        var role = await roles.FindByIdAsync(model.RoleId);
        if (role is not null) await users.AddToRoleAsync(user, role.Name!);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission("AdminUser.Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        await LoadRoles();
        return View(new AdminUserFormViewModel { Id = user.Id, FullName = user.FullName, Email = user.Email!, IsActive = user.IsActive, RoleId = (await users.GetRolesAsync(user)).FirstOrDefault() ?? "" });
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission("AdminUser.Edit")]
    public async Task<IActionResult> Edit(string id, AdminUserFormViewModel model)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (!ModelState.IsValid) { await LoadRoles(); return View(model); }
        user.FullName = model.FullName;
        user.IsActive = model.IsActive;
        await users.UpdateAsync(user);
        await users.RemoveFromRolesAsync(user, await users.GetRolesAsync(user));
        var role = await roles.FindByIdAsync(model.RoleId);
        if (role is not null) await users.AddToRoleAsync(user, role.Name!);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission("AdminUser.Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value == id) return BadRequest("Không thể khóa tài khoản đang đăng nhập.");
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        user.IsActive = false;
        await users.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRoles() => ViewBag.Roles = new SelectList(roles.Roles.OrderBy(x => x.Name), "Id", "Name");
}
