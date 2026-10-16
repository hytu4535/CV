using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public const int PageSize = 6;

        public List<Product> Products { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        public void OnGet()
        {
            var query = ProductData.All.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                query = query.Where(p => p.Category == CategoryFilter);
            }

            TotalItems = query.Count();
            Products = query
                .OrderBy(p => p.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        [IgnoreAntiforgeryToken]
        public IActionResult OnPostDelete(int id)
        {
            // In real app we would remove from data source.
            // For demo, just show success message.
            TempData["Msg"] = "Đã xóa sản phẩm thành công.";
            return RedirectToPage();
        }
    }
}