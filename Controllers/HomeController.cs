using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Helpers;
using ShopMvc.Models;
namespace ShopMvc.Controllers;
public class HomeController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string sort = "name", int page = 1) { var query = db.Products.Include(x => x.Category).Where(x => x.IsActive && !x.IsDeleted && x.Category.IsActive && !x.Category.IsDeleted).AsNoTracking(); if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search)); if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId); if (minPrice.HasValue) query = query.Where(x => (x.SalePrice ?? x.Price) >= minPrice.Value); if (maxPrice.HasValue) query = query.Where(x => (x.SalePrice ?? x.Price) <= maxPrice.Value); query = sort == "priceAsc" ? query.OrderBy(x => x.SalePrice ?? x.Price) : sort == "priceDesc" ? query.OrderByDescending(x => x.SalePrice ?? x.Price) : query.OrderBy(x => x.Name); ViewBag.Categories = await db.Categories.Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(); ViewBag.Search = search; ViewBag.CategoryId = categoryId; ViewBag.MinPrice = minPrice; ViewBag.MaxPrice = maxPrice; ViewBag.Sort = sort; return View(await PagedList<Product>.CreateAsync(query, page, 8)); }
    public async Task<IActionResult> Details(string slug) { var product = await db.Products.Include(x => x.Category).SingleOrDefaultAsync(x => x.Slug == slug && !x.IsDeleted && x.IsActive && !x.Category.IsDeleted && x.Category.IsActive); return product is null ? NotFound() : View(product); }
}
