using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Helpers;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission(Permissions.RoleView)]
public class RoleController(RoleManager<IdentityRole> roles) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 10;
        search = search?.Trim();
        var query = roles.Roles.Where(x => string.IsNullOrEmpty(search) || x.Name!.Contains(search)).OrderBy(x => x.Name);
        var total = await query.CountAsync();
        ViewBag.Search = search;
        return View(new PagedResultViewModel<IdentityRole> { Items = await query.Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToListAsync(), Page = Math.Max(page, 1), PageSize = pageSize, TotalItems = total });
    }

    [HttpGet, HasPermission(Permissions.RoleManage)]
    public IActionResult Create() => View(BuildModel());

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> Create(RoleFormViewModel model)
    {
        model.Name = model.Name.Trim();
        if (!ModelState.IsValid) return View(BuildModel(model));
        var role = new IdentityRole(model.Name);
        var result = await roles.CreateAsync(role);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), "Tên vai trò đã tồn tại hoặc không hợp lệ.");
            return View(BuildModel(model));
        }
        await SetClaims(role, model.PermissionIds);
        TempData["Success"] = "Tạo vai trò thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await roles.FindByIdAsync(id);
        if (role is null) return NotFound();
        if (role.Name == "Admin") return BadRequest("Không thể sửa vai trò Admin.");
        var claims = await roles.GetClaimsAsync(role);
        return View(BuildModel(new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name!,
            PermissionIds = claims.Where(x => x.Type == "Permission").Select(x => x.Value).ToArray(),
        }));
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> Edit(string id, RoleFormViewModel model)
    {
        var role = await roles.FindByIdAsync(id);
        if (role is null) return NotFound();
        if (role.Name == "Admin") return BadRequest("Không thể sửa vai trò Admin.");
        model.Name = model.Name.Trim();
        if (!ModelState.IsValid) return View(BuildModel(model));
        role.Name = model.Name;
        var updateResult = await roles.UpdateAsync(role);
        if (!updateResult.Succeeded) { ModelState.AddModelError(nameof(model.Name), "Tên vai trò đã tồn tại hoặc không hợp lệ."); return View(BuildModel(model)); }
        await SetClaims(role, model.PermissionIds);
        TempData["Success"] = "Cập nhật vai trò thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.RoleManage)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roles.FindByIdAsync(id);
        if (role is null) return NotFound();
        if (role.Name == "Admin") return BadRequest("Không thể xóa vai trò Admin.");
        var result = await roles.DeleteAsync(role);
        if (!result.Succeeded) return BadRequest("Không thể xóa vai trò đang được sử dụng.");
        TempData["Success"] = "Đã xóa vai trò.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetClaims(IdentityRole role, IEnumerable<string> values)
    {
        foreach (var claim in (await roles.GetClaimsAsync(role)).Where(x => x.Type == "Permission"))
            await roles.RemoveClaimAsync(role, claim);
        foreach (var value in values.Distinct())
            await roles.AddClaimAsync(role, new Claim("Permission", value));
    }

    private static RoleFormViewModel BuildModel(RoleFormViewModel? model = null)
    {
        model ??= new RoleFormViewModel();
        var selected = model.PermissionIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        model.Permissions = Permissions.All.Select(code => new PermissionOptionViewModel { Id = code, Code = code, Name = PermissionName(code), Group = code[..code.IndexOf('.')], Selected = selected.Contains(code) }).ToList();
        return model;
    }

    private static string PermissionName(string code) => code switch
    {
        Permissions.DashboardView => "Xem dashboard",
        Permissions.ProductView => "Xem sản phẩm",
        Permissions.ProductManage => "Quản lý sản phẩm",
        Permissions.CategoryView => "Xem danh mục",
        Permissions.CategoryManage => "Quản lý danh mục",
        Permissions.OrderView => "Xem đơn hàng",
        Permissions.OrderUpdate => "Cập nhật đơn hàng",
        Permissions.UserView => "Xem tài khoản",
        Permissions.UserManage => "Quản lý tài khoản",
        Permissions.RoleView => "Xem vai trò",
        Permissions.RoleManage => "Quản lý vai trò và quyền",
        _ => code
    };
}
