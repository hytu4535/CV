using System;
using System.Collections.Generic;

namespace DbFirstDemo.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal Price { get; set; }

    public decimal? SalePrice { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
