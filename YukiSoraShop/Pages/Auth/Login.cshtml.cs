using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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
        public Application.DTOs.LoginModel Input { get; set; } = new();

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
                // Tạo claims cho authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.FullName ?? account.UserName),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim(ClaimTypes.Role, account.RoleId.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in với cookie authentication
                await HttpContext.SignInAsync("CookieAuth", claimsPrincipal, new AuthenticationProperties
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

                // Set session (giữ lại cho compatibility)
                HttpContext.Session.SetString("UserEmail", account.Email);
                HttpContext.Session.SetString("UserName", account.FullName ?? account.UserName);
                HttpContext.Session.SetString("UserId", account.Id.ToString());
                HttpContext.Session.SetString("UserRole", account.RoleId.ToString());
                
                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return LocalRedirect(returnUrl ?? "/");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return Page();
        }
    }
}
