using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly ShopDbContext _context;

    public DetailsModel(ShopDbContext context) => _context = context;

    public Order Order { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Details!)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null || !CanViewOrder(order))
        {
            return NotFound();
        }

        Order = order;
        return Page();
    }

    private bool CanViewOrder(Order order)
    {
        var lastOrderId = HttpContext.Session.GetInt32("LastOrderId");
        if (lastOrderId == order.Id)
        {
            return true;
        }

        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var customerId) && customerId == order.CustomerId;
    }
}