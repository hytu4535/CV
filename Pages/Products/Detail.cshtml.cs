using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages.Products;

public class DetailsModel : PageModel
{
    public Product Product { get; set; } = new();
    public List<Product> Related { get; set; } = new();

    public IActionResult OnGet(int id)
    {
        var product = ProductData.GetById(id);
        if (product == null) return NotFound();

        Product = product;
        Related = ProductData.All
            .Where(p => p.Category == product.Category && p.Id != product.Id)
            .Take(4).ToList();

        return Page();
    }
}