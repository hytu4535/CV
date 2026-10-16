using System.Globalization;

namespace ShopUI.Models
{
    public class CartItem
    {
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; }

        public decimal Total => Product.Price * Quantity;
        public string TotalText => Total.ToString("N0", new CultureInfo("vi-VN")) + " ₫";
    }
}