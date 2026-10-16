using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Customers
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

        public List<Customer> Customers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(lowerSearch) ||
                    c.Email.ToLower().Contains(lowerSearch) ||
                    c.Phone.Contains(lowerSearch) ||
                    (c.Address != null && c.Address.ToLower().Contains(lowerSearch)));
            }

            Customers = await query
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Check if customer has any orders
            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
            if (hasOrders)
            {
                TempData["Msg"] = "Khách hàng đã có đơn hàng, không thể xóa.";
            }
            else
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Msg"] = "Đã xóa khách hàng thành công.";
            }

            return RedirectToPage();
        }
    }
}