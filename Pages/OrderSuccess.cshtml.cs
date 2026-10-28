using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;

namespace ShopUI.Pages;

public class OrderSuccessModel : PageModel
{
    private readonly ShopDbContext _context;

    public OrderSuccessModel(ShopDbContext context) => _context = context;

    public int OrderId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lastOrderId = HttpContext.Session.GetInt32("LastOrderId");
        if (lastOrderId != id || !await _context.Orders.AnyAsync(o => o.Id == id))
        {
            return RedirectToPage("/Index");
        }

        OrderId = id;
        return Page();
    }
}