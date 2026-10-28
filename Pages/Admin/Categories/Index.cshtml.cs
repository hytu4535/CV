using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly ShopDbContext _context;

    public IndexModel(ShopDbContext context) => _context = context;

    public List<Category> Categories { get; set; } = new();

    [BindProperty]
    public CategoryInput Input { get; set; } = new();

    [BindProperty]
    public int EditId { get; set; }

    [BindProperty]
    public CategoryInput EditInput { get; set; } = new();

    public class CategoryInput
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[\p{L}0-9][\p{L}0-9 &()\-]{1,99}$", ErrorMessage = "Tên danh mục chứa ký tự không hợp lệ.")]
        public string Name { get; set; } = string.Empty;
    }

    public async Task OnGetAsync() => await LoadCategoriesAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var name = Input.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError("Input.Name", "Danh mục này đã tồn tại.");
            await LoadCategoriesAsync();
            return Page();
        }

        _context.Categories.Add(new Category { Name = name });
        await _context.SaveChangesAsync();
        TempData["Msg"] = "Đã thêm danh mục thành công.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var category = await _context.Categories.FindAsync(EditId);
        if (category == null) return NotFound();

        var name = EditInput.Name.Trim();
        if (await _context.Categories.AnyAsync(c => c.Id != EditId && c.Name.ToLower() == name.ToLower()))
        {
            ModelState.AddModelError("EditInput.Name", "Danh mục này đã tồn tại.");
            await LoadCategoriesAsync();
            return Page();
        }

        category.Name = name;
        await _context.SaveChangesAsync();
        TempData["Msg"] = "Đã cập nhật danh mục thành công.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        if (await _context.Products.AnyAsync(p => p.CategoryId == id))
        {
            TempData["Error"] = "Không thể xóa danh mục đang có sản phẩm. Hãy chuyển sản phẩm sang danh mục khác trước.";
            return RedirectToPage();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa danh mục thành công.";
        return RedirectToPage();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }
}
