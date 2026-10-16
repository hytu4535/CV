using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin
{
    public class IndexModel : PageModel
    {
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
        public List<Product> TopProducts { get; set; } = new();

        public void OnGet()
        {
            // Stats
            TotalRevenue = (int)OrderData.All.Sum(o => o.Total);
            TotalOrders = OrderData.All.Count;
            TotalCustomers = CustomerData.All.Count;
            TotalProducts = ProductData.All.Count;

            // Revenue last 6 months
            var today = DateTime.Today;
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var label = month.ToString("MMM yyyy", new CultureInfo("vi-VN"));
                RevenueLabels.Add(label);
                var revenue = (int)OrderData.All
                    .Where(o => o.CreatedAt.Year == month.Year && o.CreatedAt.Month == month.Month)
                    .Sum(o => o.Total);
                RevenueValues.Add(revenue);
            }

            // Order status distribution
            var statusGroups = OrderData.All.GroupBy(o => o.Status);
            foreach (var group in statusGroups)
            {
                StatusLabels.Add(group.Key.ToString());
                StatusValues.Add(group.Count());
                StatusColors.Add(group.Key switch
                {
                    OrderStatus.Pending => "warning",
                    OrderStatus.Shipping => "info",
                    OrderStatus.Completed => "success",
                    OrderStatus.Cancelled => "danger",
                    _ => "secondary"
                });
            }

            // Latest 5 orders
            LatestOrders = OrderData.All
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToList();

            // Top 5 selling products by quantity sold
            var productQuantities = new Dictionary<int, int>();
            foreach (var order in OrderData.All)
            {
                foreach (var item in order.Items)
                {
                    if (!productQuantities.ContainsKey(item.Product.Id))
                        productQuantities[item.Product.Id] = 0;
                    productQuantities[item.Product.Id] += item.Quantity;
                }
            }
            var topProductIds = productQuantities
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .Select(kv => kv.Key)
                .ToList();
            TopProducts = ProductData.All
                .Where(p => topProductIds.Contains(p.Id))
                .OrderByDescending(p => productQuantities[p.Id])
                .ToList();
        }
    }
}