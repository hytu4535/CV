using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages;

[Authorize(Roles = "User")]
public class OrdersModel : PageModel
{
    private readonly ShopDbContext _context;

    public OrdersModel(ShopDbContext context) => _context = context;

    public List<Order> Orders { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var customerId)) return Challenge();

        Orders = await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Details!)
                .ThenInclude(d => d.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var customerId)) return Challenge();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var order = await _context.Orders
            .Include(o => o.Details!)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);

        if (order == null) return NotFound();
        if (order.Status != OrderStatus.Pending)
        {
            TempData["OrderError"] = "Chỉ có thể hủy đơn hàng khi đơn chưa được xác nhận.";
            return RedirectToPage();
        }

        foreach (var detail in order.Details ?? Enumerable.Empty<OrderDetail>())
        {
            var product = await _context.Products.FindAsync(detail.ProductId);
            if (product != null)
            {
                product.Stock += detail.Quantity;
            }
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["OrderMsg"] = $"Đã hủy đơn hàng #DH{order.Id:D4}.";
        return RedirectToPage();
    }
}
