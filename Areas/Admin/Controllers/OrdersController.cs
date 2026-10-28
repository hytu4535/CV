using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Areas.Admin.Controllers;

[Area("Admin"), HasPermission(Permissions.OrderView)]
public class OrdersController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, OrderStatus? status, int page = 1)
    {
        const int pageSize = 10;
        search = search?.Trim();
        var query = db.Orders.Include(x => x.User).Where(x => (string.IsNullOrEmpty(search) || x.Code.Contains(search) || x.User.Email!.Contains(search)) && (status == null || x.Status == status)).OrderByDescending(x => x.CreatedAt);
        var total = await query.CountAsync();
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(new PagedResultViewModel<Order> { Items = await query.Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToListAsync(), Page = Math.Max(page, 1), PageSize = pageSize, TotalItems = total });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var order = await db.Orders.Include(x => x.User).Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost, ValidateAntiForgeryToken, HasPermission(Permissions.OrderUpdate)]
    public async Task<IActionResult> Status(int id, OrderStatus? status)
    {
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        if (status is null) return BadRequest("Trạng thái đơn hàng không hợp lệ.");
        var nextStatus = status.Value;
        var valid = (order.Status == OrderStatus.Pending && nextStatus is OrderStatus.Shipping or OrderStatus.Cancelled) || (order.Status == OrderStatus.Shipping && nextStatus == OrderStatus.Completed);
        if (!valid) return BadRequest("Trạng thái đơn hàng không hợp lệ.");
        if (nextStatus == OrderStatus.Cancelled)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var ids = order.Items.Select(x => x.ProductId).Distinct().ToList();
            var products = await db.Products.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
            if (products.Count != ids.Count) return BadRequest("Không thể hoàn tồn kho cho đơn hàng.");
            foreach (var item in order.Items) products[item.ProductId].Stock += item.Quantity;
            order.Status = nextStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = "Đã hủy đơn và hoàn tồn kho thành công.";
            return RedirectToAction(nameof(Detail), "Orders", new { area = "Admin", id });
        }
        order.Status = nextStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công.";
        return RedirectToAction(nameof(Detail), "Orders", new { area = "Admin", id });
    }
}