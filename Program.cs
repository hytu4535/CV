using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using ShopUI.Data;
using ShopUI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add DbContext
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Create database if it doesn't exist and seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ShopDbContext>();
        context.Database.EnsureCreated();

        // EnsureCreated does not update an existing database when the model changes.
        // Add the columns introduced by the authentication and checkout features.
        context.Database.ExecuteSqlRaw(@"
            IF COL_LENGTH('Customers', 'PasswordHash') IS NULL
                ALTER TABLE [Customers] ADD [PasswordHash] nvarchar(255) NULL;
            IF COL_LENGTH('Orders', 'PaymentMethod') IS NULL
                ALTER TABLE [Orders] ADD [PaymentMethod] nvarchar(30) NOT NULL CONSTRAINT [DF_Orders_PaymentMethod] DEFAULT 'cod';
            IF COL_LENGTH('Orders', 'Province') IS NULL
                ALTER TABLE [Orders] ADD [Province] nvarchar(100) NULL;
            IF COL_LENGTH('Orders', 'District') IS NULL
                ALTER TABLE [Orders] ADD [District] nvarchar(100) NULL;
            IF COL_LENGTH('Orders', 'Note') IS NULL
                ALTER TABLE [Orders] ADD [Note] nvarchar(500) NULL;");

        // Seed initial data if needed
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Áo" },
                new Category { Name = "Quần" },
                new Category { Name = "Giày" },
                new Category { Name = "Phụ kiện" }
            );
            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            // Get the first category for seeding
            var category = context.Categories.FirstOrDefault();
            if (category != null)
            {
                context.Products.AddRange(
                    new Product { Name = "Áo polo nam", CategoryId = category.Id, Price = 200000, OldPrice = 250000, Stock = 50, ImageUrl = "/images/polo1.svg", Description = "Áo polo nam chất liệu cotton thoáng mát" },
                    new Product { Name = "Quần jeans nữ", CategoryId = category.Id, Price = 350000, OldPrice = 400000, Stock = 30, ImageUrl = "/images/jeans1.svg", Description = "Quần jeans nữ form skinny" },
                    new Product { Name = "Giày thể thao", CategoryId = category.Id, Price = 500000, OldPrice = 550000, Stock = 20, ImageUrl = "/images/shoe1.svg", Description = "Giày thể thao chạy bộ" }
                );
                context.SaveChanges();
            }
        }

        if (!context.Customers.Any())
        {
            context.Customers.AddRange(
                new Customer { FullName = "Nguyễn Văn A", Email = "nguyenvana@example.com", Phone = "0901234567", Address = "123 Nguyễn Văn Cừ, Q.5, TP.HCM" },
                new Customer { FullName = "Trần Thị B", Email = "tranthib@example.com", Phone = "0912345678", Address = "456 Lê Lợi, Q.1, TP.HCM" }
            );
            context.SaveChanges();
        }

        var passwordHasher = new PasswordHasher<Customer>();
        var testCustomers = new[]
        {
            new Customer { FullName = "Nguyễn Văn A", Email = "nguyenvana@example.com", Phone = "0901234567", Address = "123 Nguyễn Văn Cừ, Q.5, TP.HCM" },
            new Customer { FullName = "Trần Thị B", Email = "tranthib@example.com", Phone = "0912345678", Address = "456 Lê Lợi, Q.1, TP.HCM" }
        };
        foreach (var testCustomer in testCustomers)
        {
            var existingCustomer = context.Customers.FirstOrDefault(c => c.Email == testCustomer.Email);
            if (existingCustomer == null)
            {
                testCustomer.PasswordHash = passwordHasher.HashPassword(testCustomer, "Customer@123");
                context.Customers.Add(testCustomer);
            }
        }

        foreach (var customer in context.Customers.Where(c => c.PasswordHash == null))
        {
            customer.PasswordHash = passwordHasher.HashPassword(customer, "Customer@123");
        }
        context.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();