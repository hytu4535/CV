using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages.Products;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Sort { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int PageSize { get; } = 9;
    public int TotalPages { get; set; }

    public List<Product> Products { get; set; } = new();
    public List<string> Categories { get; set; } = new();

    public void OnGet()
    {
        Categories = ProductData.All.Select(p => p.Category).Distinct().ToList();

        var query = ProductData.All.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Category))
            query = query.Where(p => p.Category == Category);

        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase));

        query = Sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.Rating),
            _ => query.OrderBy(p => p.Id)
        };

        var totalItems = query.Count();
        TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        Products = query
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
