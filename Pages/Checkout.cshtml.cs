using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopUI.Pages;

public class CheckoutModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // Chưa lưu gì cả, chỉ báo thành công để xem giao diện
        TempData["Msg"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm tại MyShop 🎉";
        return RedirectToPage("/Index");
    }
}