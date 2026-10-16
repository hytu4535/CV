using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ShopDbContext _context;

        public IndexModel(ShopDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; } = 9;
        public int TotalPages { get; set; }

        public List<Product> Products { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Get distinct category names for dropdown
            Categories = await _context.Products
                .Select(p => p.Category.Name)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(Category))
            {
                // Filter by category name
                query = query.Where(p => p.Category.Name == Category);
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var lowerSearch = Search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(lowerSearch));
            }

            query = Sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Id) // default sort by id
            };

            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            Products = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}