using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Helpers;
using ShopMvc.Models;
namespace ShopMvc.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ShopDbContext>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var name in new[] { "Admin", "Staff", "Customer" }) if (!await roles.RoleExistsAsync(name)) await roles.CreateAsync(new IdentityRole(name));
        var admin = await roles.FindByNameAsync("Admin");
        if (admin is not null) { var claims = await roles.GetClaimsAsync(admin); foreach (var permission in Permissions.All) if (!claims.Any(x => x.Type == "Permission" && x.Value == permission)) await roles.AddClaimAsync(admin, new Claim("Permission", permission)); }
        await EnsureUser(users, "admin@shop.com", "Admin@123", "Quản trị viên", "Admin");
        await EnsureUser(users, "staff@shop.com", "Staff@123", "Nhân viên", "Staff");
        await EnsureUser(users, "khach@shop.com", "Khach@123", "Khách hàng", "Customer");
        if (!await db.Categories.AnyAsync()) { var categories = new[] { "Ví", "Túi xách", "Balo", "Thắt lưng", "Mũ", "Phụ kiện" }.Select((name, i) => new Category { Name = name, Slug = $"danh-muc-{i + 1}" }).ToList(); db.Categories.AddRange(categories); await db.SaveChangesAsync(); db.Products.AddRange(Enumerable.Range(1, 20).Select(i => new Product { Sku = $"SP-{i:000}", Name = $"Sản phẩm mẫu {i}", Slug = $"san-pham-mau-{i}", CategoryId = categories[(i - 1) % categories.Count].Id, Price = 100000 + i * 10000, Stock = 20 })); await db.SaveChangesAsync(); }
    }
    private static async Task EnsureUser(UserManager<ApplicationUser> users, string email, string password, string name, string role) { var user = await users.FindByEmailAsync(email); if (user is null) { user = new ApplicationUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true }; var result = await users.CreateAsync(user, password); if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description))); } if (!await users.IsInRoleAsync(user, role)) await users.AddToRoleAsync(user, role); }
}