using Microsoft.EntityFrameworkCore;
using ShopUI.Data;
using ShopUI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
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
                    new Product { Name = "Áo polo nam", CategoryId = category.Id, Price = 200000, OldPrice = 250000, Stock = 50, ImageUrl = "/images/polo1.jpg", Description = "Áo polo nam chất liệu cotton thoáng mát" },
                    new Product { Name = "Quần jeans nữ", CategoryId = category.Id, Price = 350000, OldPrice = 400000, Stock = 30, ImageUrl = "/images/jeans1.jpg", Description = "Quần jeans nữ form skinny" },
                    new Product { Name = "Gi thể thao", CategoryId = category.Id, Price = 500000, OldPrice = 550000, Stock = 20, ImageUrl = "/images/shoe1.jpg", Description = "Gi thể thao chạyerrrao" }
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
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();