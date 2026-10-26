using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin")]
[HasPermission("Category.View")]
public class CategoryController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Categories.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync());

    [HttpGet]
    [HasPermission("Category.Create")]
    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Category.Create")]
    public async Task<IActionResult> Create(Category model)
    {
        model.Slug = Slugify(model.Name);
        if (await db.Categories.AnyAsync(x => x.Slug == model.Slug && !x.IsDeleted)) ModelState.AddModelError(nameof(model.Name), "Tên danh mục đã tồn tại.");
        if (!ModelState.IsValid) return View(model);
        db.Categories.Add(model);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [HasPermission("Category.Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await db.Categories.FindAsync(id);
        return category is null || category.IsDeleted ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Category.Edit")]
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Category.Edit")]
    public async Task<IActionResult> Toggle(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null || category.IsDeleted) return NotFound();
        category.IsActive = !category.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Category.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        category.IsDeleted = true;
        category.IsActive = false;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant() switch { var s => string.Join('-', s.Split(' ', StringSplitOptions.RemoveEmptyEntries)) };
    }
}
