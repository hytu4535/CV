using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Orders
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = string.Empty; // empty means all

        public List<Order> Orders { get; set; } = new();
        public int TotalOrders { get; set; }
        public Dictionary<OrderStatus, int> StatusCounts { get; set; } = new();

        public void OnGet()
        {
            // Calculate status counts for all orders (for tabs)
            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                StatusCounts[status] = OrderData.All.Count(o => o.Status == status);
            }

            var query = OrderData.All.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.CustomerName.ToLower().Contains(lowerSearch) ||
                    o.Id.ToString().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (Enum.TryParse<OrderStatus>(StatusFilter, true, out var status))
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            Orders = query
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            TotalOrders = Orders.Count;
        }
    }
}