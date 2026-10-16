using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Orders
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
        public string StatusFilter { get; set; } = string.Empty; // empty means all

        public List<Order> Orders { get; set; } = new();
        public int TotalOrders { get; set; }
        public Dictionary<OrderStatus, int> StatusCounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Calculate status counts for all orders (for tabs)
            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                StatusCounts[status] = await _context.Orders.CountAsync(o => o.Status == status);
            }

            var query = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.Customer.FullName.ToLower().Contains(lowerSearch) ||
                    o.Id.ToString().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (Enum.TryParse<OrderStatus>(StatusFilter, true, out var status))
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            Orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            TotalOrders = Orders.Count;
        }
    }
}