using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Extensions;

namespace YukiSoraShop.Pages.Customer
{
    public class ViewProfileModel : PageModel
    {
        private readonly IUserService _userService;

        public ViewProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        public User? CurrentUser { get; set; }

        public void OnGet()
        {
            // Try to get user from session
            CurrentUser = HttpContext.Session.GetObject<User>("CurrentUser");

            // If no user in session, load default user
            if (CurrentUser == null)
            {
                CurrentUser = _userService.GetUserById(1);
                if (CurrentUser != null)
                {
                    HttpContext.Session.SetObject("CurrentUser", CurrentUser);
                }
            }
        }

        public IActionResult OnPostLoadUser(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user != null)
            {
                HttpContext.Session.SetObject("CurrentUser", user);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostSwitchUser(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user != null)
            {
                HttpContext.Session.SetObject("CurrentUser", user);
            }
            return RedirectToPage();
        }
    }
}