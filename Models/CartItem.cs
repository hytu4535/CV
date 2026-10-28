using System.Globalization;

namespace ShopBlazor.Models;

public class CartItem
{
    public Product Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal Total => Product.Price * Quantity;
    public string TotalText => Total.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " ₫";
}
