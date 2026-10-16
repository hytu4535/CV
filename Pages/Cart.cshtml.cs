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
                return Page();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            var cartItems = HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var existingItem = cartItems.FirstOrDefault(i => i.Product.Id == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItem { Product = product, Quantity = quantity });
            }

            HttpContext.Session.Set(CartSessionKey, cartItems);

            return RedirectToPage();
        }
    }
}