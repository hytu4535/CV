using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopUI.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Shipping = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public decimal TotalAmount { get; set; }

        [MaxLength(30)]
        public string PaymentMethod { get; set; } = "cod";

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public string StatusVietnamese => Status switch
        {
            OrderStatus.Pending => "Chờ xử lý",
            OrderStatus.Shipping => "Đang giao",
            OrderStatus.Completed => "Hoàn thành",
            OrderStatus.Cancelled => "Đã hủy",
            _ => ""
        };

        public ICollection<OrderDetail>? Details { get; set; } = new List<OrderDetail>();
    }
}