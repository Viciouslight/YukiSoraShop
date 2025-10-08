using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public Application.Models.LoginModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Thực hiện đăng nhập
            var account = await _userService.LoginAsync(Input.Email, Input.Password);
            
            if (account != null)
            {
                // Set session
                HttpContext.Session.SetString("UserEmail", account.Email);
                HttpContext.Session.SetString("UserName", account.FullName ?? account.UserName);
                HttpContext.Session.SetString("UserId", account.Id.ToString());
                HttpContext.Session.SetString("UserRole", account.Role.ToString());
                
                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return LocalRedirect(returnUrl ?? "/");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return Page();
        }
    }
}
