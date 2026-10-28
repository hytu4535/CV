using ShopBlazor.Components;
using ShopBlazor.Data;
using ShopBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ShopBlazor.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<CartState>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    db.Database.ExecuteSqlRaw("IF COL_LENGTH('Customers', 'Role') IS NULL ALTER TABLE [Customers] ADD [Role] nvarchar(20) NOT NULL CONSTRAINT [DF_Customers_Role] DEFAULT 'User'");
}

app.MapStaticAssets();

app.MapPost("/auth/login", async (HttpContext httpContext, ShopDbContext db) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.Email == email);
    var passwordValid = customer?.PasswordHash is not null &&
        new PasswordHasher<Customer>().VerifyHashedPassword(customer, customer.PasswordHash, password) != PasswordVerificationResult.Failed;

    if (!passwordValid)
        return Results.Redirect($"/login?error={Uri.EscapeDataString("Email hoặc mật khẩu không chính xác.")}");

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, customer!.Id.ToString()),
        new Claim(ClaimTypes.Name, customer.FullName),
        new Claim(ClaimTypes.Email, customer.Email),
        new Claim(ClaimTypes.Role, customer.Role)
    };
    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
        new AuthenticationProperties { IsPersistent = form["rememberMe"] == "on" });
    return Results.Redirect(IsLocalUrl(returnUrl) ? returnUrl : "/");
});

app.MapPost("/auth/admin-login", async (HttpContext httpContext, IConfiguration configuration) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();
    var adminEmail = configuration["Admin:Email"] ?? "admin@myshop.vn";
    var adminPassword = configuration["Admin:Password"] ?? "Admin@123";
    if (!string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase) || password != adminPassword)
        return Results.Redirect($"/admin?error={Uri.EscapeDataString("Email hoặc mật khẩu admin không chính xác.")}");

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, "Quản trị viên"),
        new Claim(ClaimTypes.Email, adminEmail),
        new Claim(ClaimTypes.Role, "Admin")
    };
    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
        new AuthenticationProperties { IsPersistent = form["rememberMe"] == "on" });
    return Results.Redirect("/admin/dashboard");
});

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static bool IsLocalUrl(string? url) =>
    !string.IsNullOrWhiteSpace(url) && url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\");
