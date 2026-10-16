using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages;

public class IndexModel : PageModel
{
    public List<Product> Featured { get; set; } = new();
    public List<Product> Sale { get; set; } = new();

    public void OnGet()
    {
        Featured = ProductData.All.Take(8).ToList();
        Sale = ProductData.All.Where(p => p.OldPrice.HasValue).Take(4).ToList();
    }
}