using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShopUI.Models
{
    public enum OrderStatus
    {
        Pending,
        Shipping,
        Completed,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal Total => Items.Sum(i => i.Total);

        public string StatusText => Status.ToString();

        public string StatusBadgeClass => Status switch
        {
            OrderStatus.Pending => "bg-warning text-dark",
            OrderStatus.Shipping => "bg-info text-white",
            OrderStatus.Completed => "bg-success text-white",
            OrderStatus.Cancelled => "bg-danger text-white",
            _ => "bg-secondary"
        };
    }
}