using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;
using System.ComponentModel.DataAnnotations;

namespace ShopUI.Pages.Admin.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly ShopDbContext _context;

        public DetailsModel(ShopDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; } = default!;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        public OrderStatus NewStatus { get; set; }

        public List<OrderStatus> ValidStatusTransitions => GetValidStatusTransitions();

        private List<OrderStatus> GetValidStatusTransitions()
        {
            switch (Order.Status)
            {
                case OrderStatus.Pending:
                    return new List<OrderStatus> { OrderStatus.Shipping, OrderStatus.Cancelled };
                case OrderStatus.Shipping:
                    return new List<OrderStatus> { OrderStatus.Completed, OrderStatus.Cancelled };
                case OrderStatus.Completed:
                    return new List<OrderStatus>(); // No further transitions
                case OrderStatus.Cancelled:
                    return new List<OrderStatus>(); // No further transitions
                default:
                    return new List<OrderStatus>();
            }
        }

        public string GetTransitionNotAllowedMessage()
        {
            switch (Order.Status)
            {
                case OrderStatus.Pending:
                    return "Đơn hàng ở trạng thái 'Chờ xử lý' chỉ có thể chuyển sang 'Đang giao' hoặc 'Đã hủy'.";
                case OrderStatus.Shipping:
                    return "Đơn hàng ở trạng thái 'Đang giao' chỉ có thể chuyển sang 'Hoàn thành' hoặc 'Đã hủy'.";
                case OrderStatus.Completed:
                    return "Đơn hàng đã 'Hoàn thành' và không thể thay đổi trạng thái nữa.";
                case OrderStatus.Cancelled:
                    return "Đơn hàng đã 'Đã hủy' và không thể thay đổi trạng thái nữa.";
                default:
                    return "Không thể thay đổi trạng thái này.";
            }
        }

        public string GetVietnameseStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xử lý",
                OrderStatus.Shipping => "Đang giao",
                OrderStatus.Completed => "Hoàn thành",
                OrderStatus.Cancelled => "Đã hủy",
                _ => ""
            };
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Validate status transition based on strict one-way workflow
            bool isValidTransition = false;
            switch (order.Status)
            {
                case OrderStatus.Pending:
                    // From Pending: can go to Shipping or Cancelled
                    isValidTransition = (NewStatus == OrderStatus.Shipping || NewStatus == OrderStatus.Cancelled);
                    break;
                case OrderStatus.Shipping:
                    // From Shipping: can go to Completed or Cancelled
                    isValidTransition = (NewStatus == OrderStatus.Completed || NewStatus == OrderStatus.Cancelled);
                    break;
                case OrderStatus.Completed:
                    // From Completed: no further transitions allowed
                    isValidTransition = false;
                    break;
                case OrderStatus.Cancelled:
                    // From Cancelled: no further transitions allowed
                    isValidTransition = false;
                    break;
            }

            if (!isValidTransition)
            {
                ModelState.AddModelError("NewStatus", "Không thể thay đổi trạng thái này do quy trình làm việc không cho phép.");
                return Page();
            }

            // If status is changing to Cancelled, restore stock
            if (NewStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var detail in order.Details)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                    }
                }
            }

            order.Status = NewStatus;
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Đã cập nhật trạng thái đơn hàng thành công.";
            return RedirectToPage("./Details", new { id });
        }
    }
}