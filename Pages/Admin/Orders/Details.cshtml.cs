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