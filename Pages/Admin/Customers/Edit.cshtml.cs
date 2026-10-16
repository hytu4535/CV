using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Customers
{
    public class EditModel : PageModel
    {
        private readonly ShopDbContext _context;

        public EditModel(ShopDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 2)]
            [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]{2,100}$", ErrorMessage = "Họ tên chỉ gồm chữ cái, 2-100 ký tự")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            [RegularExpression(@"^[\w\.\-]+@[\w\-]+(\.[\w\-]+)+$", ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(15)]
            [RegularExpression(@"^(0|\+84)(3|5|7|8|9)\d{8}$", ErrorMessage = "Số điện thoại VN không hợp lệ (VD: 0912345678)")]
            public string Phone { get; set; } = string.Empty;

            [StringLength(255)]
            public string? Address { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            customer.FullName = Input.FullName;
            customer.Email = Input.Email;
            customer.Phone = Input.Phone;
            customer.Address = Input.Address;

            await _context.SaveChangesAsync();

            TempData["Msg"] = "Đã cập nhật khách hàng thành công.";
            return RedirectToPage("./Index");
        }
    }
}