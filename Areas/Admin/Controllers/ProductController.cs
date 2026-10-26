using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin")]
[HasPermission("Product.View")]
public class ProductController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Products.Include(x => x.Category).Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet]
    [HasPermission("Product.Create")]
    public async Task<IActionResult> Create() { await LoadCategories(); return View(new AdminProductFormViewModel()); }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Product.Create")]
    public async Task<IActionResult> Create(AdminProductFormViewModel model)
    {
        if (!ModelState.IsValid) { await LoadCategories(); return View(model); }
        model.Name = model.Name.Trim();
        db.Products.Add(new Product
        {
            Name = model.Name,
            Price = model.Price,
            CategoryId = model.CategoryId,
            Stock = model.Stock,
            Description = model.Description,
            Slug = await CreateUniqueSlug(model.Name),
            Sku = await CreateUniqueSku()
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [HasPermission("Product.Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null || product.IsDeleted) return NotFound();
        await LoadCategories();
        return View(ToFormModel(product));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Product.Edit")]
    public async Task<IActionResult> Edit(int id, AdminProductFormViewModel model)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null || product.IsDeleted) return NotFound();
        if (!ModelState.IsValid) { await LoadCategories(); return View(model); }
        product.Name = model.Name.Trim();
        product.CategoryId = model.CategoryId;
        product.Price = model.Price;
        product.SalePrice = null;
        product.Stock = model.Stock;
        product.Description = model.Description;
        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Product.Edit")]
    public async Task<IActionResult> Toggle(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null || product.IsDeleted) return NotFound();
        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [HasPermission("Product.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.IsDeleted = true;
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategories() => ViewBag.Categories = new SelectList(await db.Categories.Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(), "Id", "Name");

    private static AdminProductFormViewModel ToFormModel(Product product) => new() { Id = product.Id, Name = product.Name, Price = product.Price, CategoryId = product.CategoryId, Stock = product.Stock, Description = product.Description };

    private async Task<string> CreateUniqueSku()
    {
        string sku;
        do sku = $"SP-{Guid.NewGuid():N}"[..13].ToUpperInvariant();
        while (await db.Products.AnyAsync(x => x.Sku == sku));
        return sku;
    }

    private async Task<string> CreateUniqueSlug(string name)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var index = 2;
        while (await db.Products.AnyAsync(x => x.Slug == slug && !x.IsDeleted)) slug = $"{baseSlug}-{index++}";
        return slug;
    }

    private static string Slugify(string value) => string.Join('-', value.Normalize(System.Text.NormalizationForm.FormD).Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray()).ToLowerInvariant().Replace(" ", "-");
}
