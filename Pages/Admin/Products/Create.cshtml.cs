using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopUI.Models;

namespace ShopUI.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Danh mục là bắt buộc")]
            public string Category { get; set; } = string.Empty;

            [Required(ErrorMessage = "Giá là bắt buộc")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
            public decimal Price { get; set; }

            public decimal? OldPrice { get; set; }

            [Required(ErrorMessage = "Mô tả là bắt buộc")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Đánh giá là bắt buộc")]
            [Range(0, 5, ErrorMessage = "Đánh giá phải từ 0 đến 5")]
            public double Rating { get; set; }

            public string ImageUrl { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // In real app we would add to data source.
            // For demo, just show success message.
            TempData["Msg"] = "Đã thêm sản phẩm thành công.";
            return RedirectToPage("./Index");
        }
    }
}