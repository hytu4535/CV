using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin")]
[HasPermission(Permissions.CategoryView)]
public class CategoryController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 10;
        search = search?.Trim();
        var query = db.Categories.Where(x => !x.IsDeleted && (string.IsNullOrEmpty(search) || x.Name.Contains(search))).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name);
        var total = await query.CountAsync();
        ViewBag.Search = search;
        return View(new PagedResultViewModel<Category> { Items = await query.Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToListAsync(), Page = Math.Max(page, 1), PageSize = pageSize, TotalItems = total });
    }

    [HttpGet]
    [HasPermission(Permissions.CategoryManage)]
    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> Create(Category model)
    {
        model.Slug = Slugify(model.Name);
        if (await db.Categories.AnyAsync(x => x.Slug == model.Slug && !x.IsDeleted)) ModelState.AddModelError(nameof(model.Name), "Tên danh mục đã tồn tại.");
        if (!ModelState.IsValid) return View(model);
        db.Categories.Add(model);
        await db.SaveChangesAsync();
        TempData["Success"] = "Tạo danh mục thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await db.Categories.FindAsync(id);
        return category is null || category.IsDeleted ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> Edit(int id, Category model)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null || category.IsDeleted) return NotFound();
        category.Name = model.Name;
        category.Description = model.Description;
        category.DisplayOrder = model.DisplayOrder;
        category.IsActive = model.IsActive;
        category.Slug = Slugify(model.Name);
        if (!ModelState.IsValid) return View(category);
        await db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật danh mục thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> Toggle(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null || category.IsDeleted) return NotFound();
        category.IsActive = !category.IsActive;
        await db.SaveChangesAsync();
        TempData["Success"] = category.IsActive ? "Đã bật danh mục." : "Đã tắt danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission(Permissions.CategoryManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        category.IsDeleted = true;
        category.IsActive = false;
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa danh mục.";
        return RedirectToAction(nameof(Index));
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant() switch { var s => string.Join('-', s.Split(' ', StringSplitOptions.RemoveEmptyEntries)) };
    }
}
