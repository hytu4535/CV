using DbFirstDemo.Models;
using Microsoft.EntityFrameworkCore;

const string connectionString = "Server=localhost;Database=ShopMvcDB;Trusted_Connection=True;TrustServerCertificate=True;";
var options = new DbContextOptionsBuilder<ShopDbFirstContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var db = new ShopDbFirstContext(options);
Console.WriteLine($"Tổng số sản phẩm: {await db.Products.CountAsync()}");
foreach (var product in await db.Products.AsNoTracking().OrderBy(x => x.Id).Take(3).ToListAsync())
    Console.WriteLine($"{product.Id}: {product.Name} - {product.Price:N0} đ");
