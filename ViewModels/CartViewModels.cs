namespace ShopMvc.ViewModels;

public sealed class CartLineViewModel
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public int Stock { get; init; }
    public decimal Total => Price * Quantity;
}

public sealed class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Lines { get; init; } = [];
    public decimal Total => Lines.Sum(x => x.Total);
}
