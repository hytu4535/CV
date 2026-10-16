using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ShopDbContext _context;

        public IndexModel(ShopDbContext context)
        {
            _context = context;
        }

        public int TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }

        public List<string> RevenueLabels { get; set; } = new();
        public List<int> RevenueValues { get; set; } = new();

        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusValues { get; set; } = new();
        public List<string> StatusColors { get; set; } = new();

        public List<Order> LatestOrders { get; set; } = new();
        public class TopProductView
        {
            public Product Product { get; set; } = default!;
            public int SoldQuantity { get; set; }
        }
        public List<TopProductView> TopProductsView { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Stats
            TotalRevenue = (int)await _context.Orders.SumAsync(o => o.TotalAmount);
            TotalOrders = await _context.Orders.CountAsync();
            TotalCustomers = await _context.Customers.CountAsync();
            TotalProducts = await _context.Products.CountAsync();

            // Revenue last 6 months
            var today = DateTime.Today;
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var label = month.ToString("MMM yyyy", new CultureInfo("vi-VN"));
                RevenueLabels.Add(label);
                var revenue = (int)await _context.Orders
                    .Where(o => o.OrderDate.Year == month.Year && o.OrderDate.Month == month.Month)
                    .SumAsync(o => o.TotalAmount);
                RevenueValues.Add(revenue);
            }

            // Order status distribution
            var statusGroups = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            foreach (var group in statusGroups)
            {
                StatusLabels.Add(group.Status.ToString());
                StatusValues.Add(group.Count);
                StatusColors.Add(group.Status switch
                {
                    OrderStatus.Pending => "warning",
                    OrderStatus.Shipping => "info",
                    OrderStatus.Completed => "success",
                    OrderStatus.Cancelled => "danger",
                    _ => "secondary"
                });
            }

            // Latest 5 orders
            LatestOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Top 5 selling products by quantity sold
            var topProductsQuery = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductId = g.Key, SoldQuantity = g.Sum(od => od.Quantity) })
                .OrderByDescending(g => g.SoldQuantity)
                .Take(5)
                .ToListAsync();

            var topProductIds = topProductsQuery.Select(p => p.ProductId).ToList();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => topProductIds.Contains(p.Id))
                .ToListAsync();

            TopProductsView = new List<TopProductView>();
            foreach (var tp in topProductsQuery)
            {
                var product = products.FirstOrDefault(p => p.Id == tp.ProductId);
                if (product != null)
                {
                    TopProductsView.Add(new TopProductView
                    {
                        Product = product,
                        SoldQuantity = tp.SoldQuantity
                    });
                }
            }
        }
    }
}