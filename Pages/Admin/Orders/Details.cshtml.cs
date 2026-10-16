using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Orders
{
    public class DetailsModel : PageModel
    {
        public Order Order { get; set; } = new();

        [BindProperty]
        public OrderStatus NewStatus { get; set; }

        public IActionResult OnGet(int id)
        {
            var order = OrderData.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }

        [IgnoreAntiforgeryToken]
        public IActionResult OnPost(int id)
        {
            var order = OrderData.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            // Update status (in memory only for demo)
            order.Status = NewStatus;

            TempData["Msg"] = "Đã cập nhật trạng thái đơn hàng thành công.";
            return RedirectToPage("./Details", new { id });
        }
    }
}