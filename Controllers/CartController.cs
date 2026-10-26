using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Controllers;

public class CartController(ShopDbContext db, UserManager<ApplicationUser> users) : Controller
{
    private const string CartKey = "ShopMvc.Cart";

    [HttpGet("cart")]
    public async Task<IActionResult> Index()
    {
        return View(await BuildCartAsync());
    }

    [HttpPost("cart/add"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == productId && x.IsActive && !x.IsDeleted);
        if (product is null) return NotFound();
        if (product.Stock <= 0)
        {
            TempData["CartError"] = "Sản phẩm đã hết hàng.";
            return RedirectToLocal(returnUrl);
        }

        var cart = GetCart();
        cart.TryGetValue(productId, out var currentQuantity);
        cart[productId] = Math.Min(product.Stock, currentQuantity + Math.Clamp(quantity, 1, 20));
        SaveCart(cart);
        TempData["CartMessage"] = $"Đã thêm {product.Name} vào giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("cart/remove"), ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = GetCart();
        cart.Remove(productId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("cart/update"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == productId && x.IsActive && !x.IsDeleted);
        var cart = GetCart();
        if (product is null || product.Stock <= 0)
            cart.Remove(productId);
        else
            cart[productId] = Math.Clamp(quantity, 1, product.Stock);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("cart/checkout")]
    public async Task<IActionResult> Checkout()
    {
        if (!User.Identity.IsAuthenticated)
            return Redirect("/account/login?returnUrl=/cart/checkout");

        var customer = await users.FindByIdAsync(users.GetUserId(User)!);
        if (customer is null) return Redirect("/account/login?returnUrl=/cart/checkout");
        if (GetCart().Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng đang trống.";
            return RedirectToAction(nameof(Index));
        }

        return View(new CheckoutViewModel
        {
            FullName = customer.FullName,
            Phone = customer.PhoneNumber ?? string.Empty,
            Address = string.Empty,
            Email = customer.Email ?? string.Empty
        });
    }

    [HttpPost("cart/checkout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!User.Identity.IsAuthenticated)
            return Redirect("/account/login?returnUrl=/cart/checkout");
        if (!ModelState.IsValid) return View(model);

        var cart = GetCart();
        if (cart.Count == 0)
        {
            TempData["CartError"] = "Giỏ hàng đang trống.";
            return RedirectToAction(nameof(Index));
        }

        var products = await db.Products.Where(x => cart.Keys.Contains(x.Id) && x.IsActive && !x.IsDeleted).ToDictionaryAsync(x => x.Id);
        foreach (var item in cart)
        {
            if (!products.TryGetValue(item.Key, out var product) || product.Stock < item.Value)
            {
                TempData["CartError"] = "Một sản phẩm trong giỏ không đủ tồn kho.";
                return RedirectToAction(nameof(Index));
            }
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        foreach (var item in cart) products[item.Key].Stock -= item.Value;
        var customer = await users.FindByIdAsync(users.GetUserId(User)!);
        if (customer is null) return Challenge();
        customer.FullName = model.FullName;
        customer.PhoneNumber = model.Phone;
        await users.UpdateAsync(customer);
        var order = new Order { Code = $"DH{DateTime.Now:yyyyMMddHHmmssfff}", UserId = customer.Id, ReceiverName = model.FullName, Phone = model.Phone, Address = model.Address, Note = model.Note, TotalAmount = products.Sum(x => x.Value.CurrentPrice * cart[x.Key]) };
        order.Items = cart.Select(x => new OrderItem { ProductId = x.Key, ProductName = products[x.Key].Name, UnitPrice = products[x.Key].CurrentPrice, Quantity = x.Value }).ToList();
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        HttpContext.Session.Remove(CartKey);
        TempData["CartMessage"] = "Đặt hàng thành công. Cảm ơn bạn đã mua hàng.";
        return RedirectToAction("Detail", "Orders", new { id = order.Id });
    }

    private async Task<CartViewModel> BuildCartAsync()
    {
        var cart = GetCart();
        if (cart.Count == 0) return new CartViewModel();

        var products = await db.Products.AsNoTracking().Where(x => cart.Keys.Contains(x.Id) && x.IsActive && !x.IsDeleted).ToListAsync();
        var lines = products.Select(product => new CartLineViewModel
        {
            ProductId = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Price = product.CurrentPrice,
            Quantity = Math.Min(cart[product.Id], product.Stock),
            Stock = product.Stock
        }).Where(x => x.Quantity > 0).ToList();
        return new CartViewModel { Lines = lines };
    }

    private Dictionary<int, int> GetCart()
    {
        var json = HttpContext.Session.GetString(CartKey);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? [];
    }

    private void SaveCart(Dictionary<int, int> cart) => HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));

    private IActionResult RedirectToLocal(string? returnUrl) => Url.IsLocalUrl(returnUrl) && !returnUrl!.StartsWith("/admin", StringComparison.OrdinalIgnoreCase)
        ? Redirect(returnUrl)
        : RedirectToAction(nameof(Index));
}
