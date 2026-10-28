using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission(Permissions.UserView)]
public class AdminUserController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 10;
        search = search?.Trim();
        var query = users.Users.AsNoTracking().Where(x => string.IsNullOrEmpty(search) || x.FullName.Contains(search) || x.Email!.Contains(search)).OrderBy(x => x.FullName);
        var total = await query.CountAsync();
        var pageItems = await query.Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = new List<AdminUserListItemViewModel>();
        foreach (var user in pageItems) items.Add(new AdminUserListItemViewModel { User = user, Roles = (await users.GetRolesAsync(user)).ToList() });
        ViewBag.Search = search;
        return View(new PagedResultViewModel<AdminUserListItemViewModel> { Items = items, Page = Math.Max(page, 1), PageSize = pageSize, TotalItems = total });
    }

    [HttpGet, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> Create() { await LoadRoles(); return View(new AdminUserFormViewModel()); }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> Create(AdminUserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password)) ModelState.AddModelError(nameof(model.Password), "Mật khẩu là bắt buộc khi tạo tài khoản.");
        if (await users.FindByEmailAsync(model.Email) is not null) ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
        if (await roles.FindByIdAsync(model.RoleId) is null) ModelState.AddModelError(nameof(model.RoleId), "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) { await LoadRoles(); return View(model); }
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, IsActive = model.IsActive, EmailConfirmed = true };
        var result = await users.CreateAsync(user, model.Password!);
        if (!result.Succeeded) { AddErrors(result); await LoadRoles(); return View(model); }
        var role = await roles.FindByIdAsync(model.RoleId);
        await users.AddToRoleAsync(user, role!.Name!);
        TempData["Success"] = "Tạo tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        await LoadRoles();
        return View(new AdminUserFormViewModel { Id = user.Id, FullName = user.FullName, Email = user.Email!, IsActive = user.IsActive, RoleId = (await users.GetRolesAsync(user)).FirstOrDefault() ?? "" });
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> Edit(string id, AdminUserFormViewModel model)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        ModelState.Remove(nameof(model.Email));
        if (string.IsNullOrWhiteSpace(model.Password)) { ModelState.Remove(nameof(model.Password)); ModelState.Remove(nameof(model.ConfirmPassword)); }
        if (UserId == id && !model.IsActive) ModelState.AddModelError(nameof(model.IsActive), "Bạn không thể tự khóa tài khoản đang đăng nhập.");
        if (await roles.FindByIdAsync(model.RoleId) is null) ModelState.AddModelError(nameof(model.RoleId), "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) { await LoadRoles(); return View(model); }
        user.FullName = model.FullName;
        user.IsActive = model.IsActive;
        var updateResult = await users.UpdateAsync(user);
        if (!updateResult.Succeeded) { AddErrors(updateResult); await LoadRoles(); return View(model); }
        await users.RemoveFromRolesAsync(user, await users.GetRolesAsync(user));
        var role = await roles.FindByIdAsync(model.RoleId);
        await users.AddToRoleAsync(user, role!.Name!);
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await users.ResetPasswordAsync(user, token, model.Password);
            if (!passwordResult.Succeeded) { AddErrors(passwordResult); await LoadRoles(); return View(model); }
        }
        TempData["Success"] = "Cập nhật tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> Toggle(string id)
    {
        if (UserId == id) return BadRequest("Không thể tự khóa hoặc mở khóa tài khoản đang đăng nhập.");
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        user.IsActive = !user.IsActive;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(string.Join(" ", result.Errors.Select(x => x.Description)));
        TempData["Success"] = user.IsActive ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> Delete(string id)
    {
        if (UserId == id) return BadRequest("Không thể xóa tài khoản đang đăng nhập.");
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        var result = await users.DeleteAsync(user);
        if (!result.Succeeded) return BadRequest(string.Join(" ", result.Errors.Select(x => x.Description)));
        TempData["Success"] = "Đã xóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await users.FindByIdAsync(id);
        return user is null ? NotFound() : View(new ResetPasswordViewModel { Id = id });
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.UserManage)]
    public async Task<IActionResult> ResetPassword(string id, ResetPasswordViewModel model)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null) return NotFound();
        if (!ModelState.IsValid) return View(model);
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var result = await users.ResetPasswordAsync(user, token, model.Password);
        if (!result.Succeeded) { AddErrors(result); return View(model); }
        TempData["Success"] = "Đặt lại mật khẩu thành công.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRoles() => ViewBag.Roles = new SelectList(roles.Roles.OrderBy(x => x.Name), "Id", "Name");
    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    private void AddErrors(IdentityResult result) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); }
}
