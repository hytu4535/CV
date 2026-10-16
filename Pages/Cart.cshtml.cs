using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Data;
using ShopUI.Models;

namespace ShopUI.Pages;

public class CartModel : PageModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(i => i.Total);
    public decimal Shipping => SubTotal >= 500000 ? 0 : 30000;
    public decimal GrandTotal => SubTotal + Shipping;

    public void OnGet()
    {
        // Dữ liệu giả để dựng giao diện (chưa có logic giỏ hàng thật)
        Items = new List<CartItem>
        {
            new() { Product = ProductData.All[0], Quantity = 1 },
            new() { Product = ProductData.All[3], Quantity = 2 },
            new() { Product = ProductData.All[8], Quantity = 1 },
        };
    }
}