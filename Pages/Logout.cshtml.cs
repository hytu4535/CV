using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShopUI.Pages;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync() => await SignOutAsync();
    public async Task<IActionResult> OnPostAsync() => await SignOutAsync();

    private async Task<IActionResult> SignOutAsync()
    {
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
