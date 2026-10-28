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
        public string status { get; set; } = string.Empty; // empty means all

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; } = 10;
        public int TotalPages { get; set; }
        public int TotalOrders { get; set; }
        public int PendingCount { get; set; }
        public int ShippingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public List<Order> Orders { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.Customer.FullName.ToLower().Contains(lowerSearch) ||
                    o.Id.ToString().Contains(lowerSearch));
            }

            // Handle status filtering with mapping between Vietnamese and English
            if (!string.IsNullOrWhiteSpace(status))
            {
                // Map Vietnamese status values to OrderStatus enum
                OrderStatus? filterStatus = null;

                switch (status.ToLower())
                {
                    case "pending":
                    case "chờ xử lý":
                        filterStatus = OrderStatus.Pending;
                        break;
                    case "shipping":
                    case "đang giao":
                        filterStatus = OrderStatus.Shipping;
                        break;
                    case "completed":
                    case "hoàn thành":
                        filterStatus = OrderStatus.Completed;
                        break;
                    case "cancelled":
                    case "đã hủy":
                        filterStatus = OrderStatus.Cancelled;
                        break;
                    case "all":
                    case "tất cả":
                        filterStatus = null; // No filtering
                        break;
                }

                // Apply filter if we have a valid status
                if (filterStatus.HasValue)
                {
                    query = query.Where(o => o.Status == filterStatus.Value);
                }
                // If status doesn't match any known value, we could optionally fall back to Enum.TryParse
                else if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                {
                    query = query.Where(o => o.Status == parsedStatus);
                }
            }

            // Apply date range filtering
            if (StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date >= StartDate.Value.Date);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate.Date <= EndDate.Value.Date);
            }

            // Get total count for pagination
            TotalOrders = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalOrders / (double)PageSize);

            // Apply pagination
            Orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Pre-calculate count variables for the view (after filtering)
            PendingCount = await _context.Orders.CountAsync(o =>
                (string.IsNullOrWhiteSpace(SearchTerm) ||
                 (o.Customer.FullName.ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                  o.Id.ToString().Contains(SearchTerm.Trim().ToLower()))) &&
                (!StartDate.HasValue || o.OrderDate.Date >= StartDate.Value.Date) &&
                (!EndDate.HasValue || o.OrderDate.Date <= EndDate.Value.Date) &&
                (string.IsNullOrWhiteSpace(status) ||
                 status.ToLower() == "all" || status.ToLower() == "tất cả" ||
                 status.ToLower() == "pending" || status.ToLower() == "chờ xử lý") &&
                o.Status == OrderStatus.Pending);

            ShippingCount = await _context.Orders.CountAsync(o =>
                (string.IsNullOrWhiteSpace(SearchTerm) ||
                 (o.Customer.FullName.ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                  o.Id.ToString().Contains(SearchTerm.Trim().ToLower()))) &&
                (!StartDate.HasValue || o.OrderDate.Date >= StartDate.Value.Date) &&
                (!EndDate.HasValue || o.OrderDate.Date <= EndDate.Value.Date) &&
                (string.IsNullOrWhiteSpace(status) ||
                 status.ToLower() == "all" || status.ToLower() == "tất cả" ||
                 status.ToLower() == "shipping" || status.ToLower() == "đang giao") &&
                o.Status == OrderStatus.Shipping);

            CompletedCount = await _context.Orders.CountAsync(o =>
                (string.IsNullOrWhiteSpace(SearchTerm) ||
                 (o.Customer.FullName.ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                  o.Id.ToString().Contains(SearchTerm.Trim().ToLower()))) &&
                (!StartDate.HasValue || o.OrderDate.Date >= StartDate.Value.Date) &&
                (!EndDate.HasValue || o.OrderDate.Date <= EndDate.Value.Date) &&
                (string.IsNullOrWhiteSpace(status) ||
                 status.ToLower() == "all" || status.ToLower() == "tất cả" ||
                 status.ToLower() == "completed" || status.ToLower() == "hoàn thành") &&
                o.Status == OrderStatus.Completed);

            CancelledCount = await _context.Orders.CountAsync(o =>
                (string.IsNullOrWhiteSpace(SearchTerm) ||
                 (o.Customer.FullName.ToLower().Contains(SearchTerm.Trim().ToLower()) ||
                  o.Id.ToString().Contains(SearchTerm.Trim().ToLower()))) &&
                (!StartDate.HasValue || o.OrderDate.Date >= StartDate.Value.Date) &&
                (!EndDate.HasValue || o.OrderDate.Date <= EndDate.Value.Date) &&
                (string.IsNullOrWhiteSpace(status) ||
                 status.ToLower() == "all" || status.ToLower() == "tất cả" ||
                 status.ToLower() == "cancelled" || status.ToLower() == "đã hủy") &&
                o.Status == OrderStatus.Cancelled);
        }
    }
}