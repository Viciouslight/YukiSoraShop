using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // Sign out khỏi authentication
            await HttpContext.SignOutAsync("CookieAuth");
            
            // Xóa session
            HttpContext.Session.Clear();
            
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToPage("/Home");
        }
    }
}
