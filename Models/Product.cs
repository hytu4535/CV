using System;
using System.Globalization;

namespace ShopUI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Rating { get; set; }

        private static readonly CultureInfo Vn = new CultureInfo("vi-VN");

        public string PriceText => Price.ToString("N0", Vn) + " ₫";
        public string OldPriceText => OldPrice.HasValue ? OldPrice.Value.ToString("N0", Vn) + " ₫" : "";
        public int DiscountPercent =>
            OldPrice.HasValue && OldPrice > 0 ? (int)Math.Round((1 - Price / OldPrice.Value) * 100) : 0;
    }
}