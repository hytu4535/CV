using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopUI.Models;
using ShopUI.Data;

namespace ShopUI.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        private readonly ShopDbContext _context;

        public CreateModel(ShopDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(150, MinimumLength = 3)]
            [RegularExpression(@"^[a-zA-Z0-9À-ỹ\s\-\.,()]{3,150}$", ErrorMessage = "Tên 3-150 ký tự, chỉ gồm chữ, số và các ký tự - . , ( )")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
            public int CategoryId { get; set; }

            [Required]
            [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1000 đến 1.000.000.000")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Tồn kho không được để trống.")]
            [Range(0, 100000, ErrorMessage = "Tồn kho phải từ 0 đến 100.000")]
            public int? Stock { get; set; }

            [StringLength(500)]
            [RegularExpression(@"^(https?:\/\/.+\.(jpg|jpeg|png|webp|gif).*)?$", ErrorMessage = "URL hình ảnh không hợp lệ")]
            public string? ImageUrl { get; set; }

            [Required(ErrorMessage = "Mô tả không được để trống.")]
            [StringLength(1000)]
            public string? Description { get; set; }

            [GreaterThanProperty("Price", ErrorMessage = "Giá cũ phải lớn hơn giá bán hiện tại.")]
            [Range(1000, 1000000000, ErrorMessage = "Giá cũ phải từ 1000 đến 1.000.000.000")]
            public decimal? OldPrice { get; set; }
        }

        public SelectList CategoryList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            CategoryList = new SelectList(categories, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // reload categories
                return Page();
            }

            var product = new Product
            {
                Name = Input.Name,
                CategoryId = Input.CategoryId,
                Price = Input.Price,
                Stock = Input.Stock.Value,
                ImageUrl = Input.ImageUrl,
                Description = Input.Description,
                OldPrice = Input.OldPrice
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Đã thêm sản phẩm thành công.";
            return RedirectToPage("./Index");
        }
    }
}