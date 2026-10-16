using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly ShopDbContext _context;

        public IndexModel(ShopDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public const int PageSize = 8;

        public List<Product> Products { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Lấy danh sách danh mục để hiển thị trong dropdown lọc
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                // Lọc theo tên danh mục
                query = query.Where(p => p.Category.Name.Contains(CategoryFilter, StringComparison.OrdinalIgnoreCase));
            }

            TotalItems = await query.CountAsync();
            Products = await query
                .OrderBy(p => p.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Msg"] = "Đã xóa sản phẩm thành công.";
            }
            catch (DbUpdateException ex)
            {
                // Nếu có ràng buộc khóa ngoại (sản phẩm có trong đơn hàng)
                TempData["Msg"] = "Không thể xóa sản phẩm vì sản phẩm đã có trong đơn hàng.";
            }
            catch (Exception)
            {
                TempData["Msg"] = "Có lỗi xảy ra khi xóa sản phẩm.";
            }

            return RedirectToPage();
        }
    }
}