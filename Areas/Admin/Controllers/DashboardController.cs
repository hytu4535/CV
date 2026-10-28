using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;
using ShopMvc.ViewModels;
namespace ShopMvc.Areas.Admin.Controllers;
[Area("Admin"), HasPermission(Permissions.DashboardView)]
public class DashboardController(ShopDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(new DashboardViewModel
        {
            ProductCount = await db.Products.CountAsync(x => !x.IsDeleted),
            CategoryCount = await db.Categories.CountAsync(x => !x.IsDeleted),
            OrderCount = await db.Orders.CountAsync(),
            AccountCount = await users.Users.CountAsync(),
            LatestOrders = await db.Orders.Include(x => x.User).OrderByDescending(x => x.CreatedAt).Take(10).ToListAsync()
        });
    }
}
