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
            [StringLength(150, MinimumLength = 3)]
            [RegularExpression(@"^[a-zA-Z0-9À-ỹ\s\-\.,()]{3,150}$", ErrorMessage = "Tên 3-150 ký tự, chỉ gồm chữ, số và các ký tự - . , ( )")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
            public int CategoryId { get; set; }

            [Required]
            [Range(1000, 1000000000, ErrorMessage = "Giá phải từ 1000 đến 1.000.000.000")]
            public decimal Price { get; set; }

            [GreaterThanProperty("Price")]
            public decimal? OldPrice { get; set; }

            [Required(ErrorMessage = "Tồn kho không được để trống.")]
            [Range(0, 100000, ErrorMessage = "Tồn kho phải từ 0 đến 100.000")]
            public int? Stock { get; set; }

            [StringLength(500)]
            [RegularExpression(@"^(https?:\/\/.+\.(jpg|jpeg|png|webp|gif).*)?$", ErrorMessage = "URL hình ảnh không hợp lệ")]
            public string? ImageUrl { get; set; }

            [Required(ErrorMessage = "Mô tả không được để trống.")]
            [StringLength(1000)]
            public string? Description { get; set; }
        }

        public SelectList CategoryList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                Description = product.Description
            };

            await LoadCategoriesAsync();
            return Page();
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            CategoryList = new SelectList(categories, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            // Additional validation: OldPrice > Price if OldPrice has value
            if (Input.OldPrice.HasValue && Input.OldPrice <= Input.Price)
            {
                ModelState.AddModelError("Input.OldPrice", "Giá cũ phải lớn hơn giá mới.");
                await LoadCategoriesAsync();
                return Page();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = Input.Name;
            product.CategoryId = Input.CategoryId;
            product.Price = Input.Price;
            product.OldPrice = Input.OldPrice;
            product.Stock = Input.Stock.Value;
            product.ImageUrl = Input.ImageUrl;
            product.Description = Input.Description;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Msg"] = "Đã cập nhật sản phẩm thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Msg"] = "Có lỗi xảy ra khi cập nhật sản phẩm.";
            }

            return RedirectToPage("./Index");
        }
    }
}