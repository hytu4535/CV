using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ShopDbContext _context;

        public IndexModel(ShopDbContext context)
        {
            _context = context;
        }

        public List<Product> Featured { get; set; } = new();
        public List<Product> Sale { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Lấy 8 sản phẩm nổi bật (sắp xếp theo Id giảm dần hoặc mới nhất)
            Featured = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            // Lấy sản phẩm giảm giá (OldPrice > Price)
            Sale = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.OldPrice.HasValue && p.OldPrice > p.Price)
                .Take(4)
                .ToListAsync();
        }
    }
}