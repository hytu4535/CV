using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Customers
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        public List<Customer> Customers { get; set; } = new();

        public void OnGet()
        {
            var query = CustomerData.All.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerSearch = SearchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(lowerSearch) ||
                    c.Email.ToLower().Contains(lowerSearch) ||
                    c.Phone.Contains(lowerSearch) ||
                    c.City.ToLower().Contains(lowerSearch));
            }

            Customers = query
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}