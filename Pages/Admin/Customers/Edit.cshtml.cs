using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

            [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự.")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
            [RegularExpression(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", ErrorMessage = "Email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
            [RegularExpression(@"^(0|\+84)(3|5|7|8|9)\d{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ (VD: 0912345678).")]
            public string Phone { get; set; } = string.Empty;

            [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var email = Input.Email.Trim();
            var phone = Input.Phone.Trim();
            if (await _context.Customers.AnyAsync(c => c.Id != id && c.Email.ToLower() == email.ToLower()))
            {
                ModelState.AddModelError("Input.Email", "Email này đã được sử dụng.");
            }
            if (await _context.Customers.AnyAsync(c => c.Id != id && c.Phone == phone))
            {
                ModelState.AddModelError("Input.Phone", "Số điện thoại này đã được sử dụng.");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            customer.FullName = Input.FullName.Trim();
            customer.Email = email;
            customer.Phone = phone;
            customer.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();

            await _context.SaveChangesAsync();

            TempData["Msg"] = "Đã cập nhật khách hàng thành công.";
            return RedirectToPage("./Index");
        }
    }
}