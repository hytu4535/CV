using System;
using System.Collections.Generic;

namespace DbFirstDemo.Models;

public partial class Order
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string ReceiverName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Note { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual AspNetUser User { get; set; } = null!;
}
