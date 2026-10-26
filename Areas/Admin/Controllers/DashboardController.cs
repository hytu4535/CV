using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;
namespace ShopMvc.Areas.Admin.Controllers;
[Area("Admin"), HasPermission("Dashboard.View")]
public class DashboardController(ShopDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index() { ViewBag.Products = await db.Products.CountAsync(x => !x.IsDeleted && x.IsActive); ViewBag.Hidden = await db.Products.CountAsync(x => !x.IsDeleted && !x.IsActive); ViewBag.Categories = await db.Categories.CountAsync(); ViewBag.Admins = (await users.GetUsersInRoleAsync("Admin")).Count; ViewBag.Customers = (await users.GetUsersInRoleAsync("Customer")).Count; ViewBag.Low = await db.Products.Where(x => x.Stock <= 5 && !x.IsDeleted).OrderBy(x => x.Stock).ToListAsync(); ViewBag.Logs = Array.Empty<object>(); return View(); }
}
