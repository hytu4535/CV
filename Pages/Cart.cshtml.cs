using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;
using ShopUI;

namespace ShopUI.Pages
{
    public class CartModel : PageModel
    {
        private const string CartSessionKey = "Cart";
        private readonly ShopDbContext _context;

        public CartModel(ShopDbContext context)
        {
            _context = context;
        }

        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.Total);
        public decimal Shipping => SubTotal >= 500000 ? 0 : 30000;
        public decimal GrandTotal => SubTotal + Shipping;

        public async Task OnGetAsync()
        {
            // Load cart from session
            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            Items = cartItems;
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["CartError"] = "Số lượng sản phẩm không hợp lệ.";
                return RedirectToPage();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Stock <= 0)
            {
                TempData["CartError"] = $"Sản phẩm '{product.Name}' đã hết hàng.";
                return RedirectToPage();
            }

            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var existingItem = cartItems.FirstOrDefault(i => i.Product.Id == productId);
            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    existingItem.Quantity = product.Stock;
                    TempData["CartError"] = $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} sản phẩm trong kho.";
                }
                else
                {
                    existingItem.Quantity = newQuantity;
                }
            }
            else
            {
                var allowedQuantity = Math.Min(quantity, product.Stock);
                cartItems.Add(new CartItem { Product = product, Quantity = allowedQuantity });
                if (allowedQuantity < quantity)
                {
                    TempData["CartError"] = $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} sản phẩm trong kho.";
                }
            }

            HttpContext.Session.Set(CartSessionKey, cartItems);

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveFromCart(int productId)
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cartItems.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                cartItems.Remove(item);
                HttpContext.Session.Set(CartSessionKey, cartItems);
                TempData["CartMsg"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, int quantity)
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cartItems.FirstOrDefault(i => i.Product.Id == productId);
            if (item == null)
            {
                return RedirectToPage();
            }

            if (quantity <= 0)
            {
                TempData["CartError"] = "Số lượng sản phẩm không hợp lệ.";
                return RedirectToPage();
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                cartItems.Remove(item);
                HttpContext.Session.Set(CartSessionKey, cartItems);
                TempData["CartError"] = "Sản phẩm không còn tồn tại.";
                return RedirectToPage();
            }

            if (quantity > product.Stock)
            {
                item.Quantity = product.Stock;
                TempData["CartError"] = $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} sản phẩm trong kho.";
            }
            else
            {
                item.Quantity = quantity;
                TempData["CartMsg"] = "Đã cập nhật số lượng sản phẩm.";
            }

            HttpContext.Session.Set(CartSessionKey, cartItems);
            return RedirectToPage();
        }
    }
}