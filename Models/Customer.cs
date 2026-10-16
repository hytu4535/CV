using System;
using System.Globalization;

namespace ShopUI.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}