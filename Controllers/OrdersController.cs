using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Models;

namespace ShopMvc.Controllers;

[Route("account/orders")]
public class OrdersController(ShopDbContext db, UserManager<ApplicationUser> users) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return Redirect("/account/login?returnUrl=/account/orders");
        var id = users.GetUserId(User)!;
        return View(await db.Orders.Where(x => x.UserId == id).OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id && x.UserId == users.GetUserId(User));
        return order is null ? NotFound() : View(order);
    }

    [HttpPost("{id:int}/cancel"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id && x.UserId == users.GetUserId(User));
        if (order is null) return NotFound();
        if (order.Status != OrderStatus.Pending) return BadRequest("Chỉ được hủy đơn đang chờ xử lý.");
        var ids = order.Items.Select(x => x.ProductId).ToList();
        var products = await db.Products.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        foreach (var item in order.Items) if (products.TryGetValue(item.ProductId, out var product)) product.Stock += item.Quantity;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Detail), new { id });
    }
}