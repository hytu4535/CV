using Microsoft.EntityFrameworkCore;
using ShopBlazor.Models;

namespace ShopBlazor.Data;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        modelBuilder.Entity<Customer>().Property(c => c.Role).HasMaxLength(20).HasDefaultValue("User");
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 0);
        modelBuilder.Entity<Product>().Property(p => p.OldPrice).HasPrecision(18, 0);
        modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 0);
        modelBuilder.Entity<Order>().Property(o => o.PaymentMethod).HasMaxLength(30);
        modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasPrecision(18, 0);
        modelBuilder.Entity<OrderDetail>().HasOne(od => od.Order).WithMany(o => o.Details).HasForeignKey(od => od.OrderId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderDetail>().HasOne(od => od.Product).WithMany().HasForeignKey(od => od.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
