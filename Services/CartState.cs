using ShopBlazor.Models;

namespace ShopBlazor.Services;

public class CartState
{
    public List<CartItem> Items { get; } = new();
    public decimal SubTotal => Items.Sum(item => item.Total);
    public decimal Shipping => SubTotal >= 500000 ? 0 : 30000;
    public decimal GrandTotal => SubTotal + Shipping;

    public void Add(Product product, int quantity)
    {
        var item = Items.FirstOrDefault(x => x.Product.Id == product.Id);
        if (item is null) Items.Add(new CartItem { Product = product, Quantity = Math.Min(quantity, product.Stock) });
        else item.Quantity = Math.Min(item.Quantity + quantity, product.Stock);
    }

    public void Remove(int productId) => Items.RemoveAll(x => x.Product.Id == productId);
}
