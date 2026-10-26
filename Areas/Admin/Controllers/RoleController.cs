using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopMvc.Helpers;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission("Role.View")]
public class RoleController(RoleManager<IdentityRole> roles) : Controller
{
    public async Task<IActionResult> Index() => View(roles.Roles.OrderBy(x => x.Name).ToList());

    [HttpGet]
    [HasPermission("Role.Manage")]
    public IActionResult Create() => View(new RoleFormViewModel { Permissions = Permissions.All.Select(x => new PermissionOptionViewModel { Code = x }).ToList() });

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Role.Manage")]
    public async Task<IActionResult> Create(RoleFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var role = new IdentityRole(model.Name);
        var result = await roles.CreateAsync(role);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Name), "Tên vai trò đã tồn tại.");
            return View(model);
        }
        await SetClaims(role, model.PermissionIds);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [HasPermission("Role.Manage")]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await roles.FindByIdAsync(id);
        if (role is null) return NotFound();
        var claims = await roles.GetClaimsAsync(role);
        return View(new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name!,
            PermissionIds = claims.Where(x => x.Type == "Permission").Select(x => x.Value).ToArray(),
            Permissions = Permissions.All.Select(x => new PermissionOptionViewModel { Code = x, Selected = claims.Any(c => c.Value == x) }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Role.Manage")]
    public async Task<IActionResult> Edit(string id, RoleFormViewModel model)
    {
        var role = await roles.FindByIdAsync(id);
        if (role is null) return NotFound();
        if (role.Name == "Admin") return BadRequest("Không thể sửa vai trò Admin.");
        if (!ModelState.IsValid) return View(model);
        role.Name = model.Name;
        await roles.UpdateAsync(role);
        await SetClaims(role, model.PermissionIds);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Role.Manage")]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roles.FindByIdAsync(id);
        if (role is null) return NotFound();
        if (role.Name == "Admin") return BadRequest("Không thể xóa vai trò Admin.");
        await roles.DeleteAsync(role);
        return RedirectToAction(nameof(Index));
    }

    private async Task SetClaims(IdentityRole role, IEnumerable<string> values)
    {
        foreach (var claim in await roles.GetClaimsAsync(role))
            await roles.RemoveClaimAsync(role, claim);
        foreach (var value in values.Distinct())
            await roles.AddClaimAsync(role, new Claim("Permission", value));
    }
}
