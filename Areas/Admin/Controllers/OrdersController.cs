using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission("Order.View")]
public class OrdersController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index(OrderStatus? status) => View(await db.Orders.Include(x => x.User).Where(x => status == null || x.Status == status).OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken, HasPermission("Order.Update")]
    public async Task<IActionResult> Status(int id, OrderStatus status)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        var valid = order.Status == OrderStatus.Pending && status is OrderStatus.Shipping or OrderStatus.Cancelled || order.Status == OrderStatus.Shipping && status == OrderStatus.Completed;
        if (!valid) return BadRequest("Trạng thái đơn hàng không hợp lệ.");
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}