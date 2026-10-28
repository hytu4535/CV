using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.Cookie.Name = ".Shop.Session");
builder.Services.AddDbContext<ShopDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ShopDbContext>()
.AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".Shop.Identity";
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.Events.OnRedirectToLogin = context =>
    {
        var path = context.Request.Path.StartsWithSegments("/admin") ? "/admin/login" : "/account/login";
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"{path}?returnUrl={Uri.EscapeDataString(returnUrl)}");
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        var path = context.Request.Path.StartsWithSegments("/admin") ? "/admin/Account/AccessDenied" : "/account/AccessDenied";
        context.Response.Redirect(path);
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    try { await db.Database.MigrateAsync(); await DbSeeder.SeedAsync(scope.ServiceProvider); }
    catch (Exception ex) { scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup").LogError(ex, "Không thể khởi tạo cơ sở dữ liệu."); }
}
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("admin-auth", "admin/login", new { area = "Admin", controller = "Account", action = "Login" });
app.MapControllerRoute("admin-products", "admin/products/{action=Index}/{id?}", new { area = "Admin", controller = "Product" });
app.MapControllerRoute("admin", "admin/{controller=Dashboard}/{action=Index}/{id?}", new { area = "Admin" });
app.MapControllerRoute("account", "{controller=Account}/{action=Login}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
