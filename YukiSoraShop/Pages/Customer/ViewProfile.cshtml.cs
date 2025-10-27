using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YukiSoraShop.Extensions;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class ViewProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<ViewProfileModel> _logger;

        public ViewProfileModel(IUserService userService, ILogger<ViewProfileModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public UserDto? CurrentUser { get; set; }

        public void OnGet()
        {
            try
            {
                // Get user info from login session
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var userName = HttpContext.Session.GetString("UserName");
                var userId = HttpContext.Session.GetString("UserId");
                
                // Debug: Log session data
                _logger.LogDebug("Session - Email: {Email}, Name: {Name}, ID: {Id}", userEmail, userName, userId);

                if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(userId))
                {
                    // Create user object from session data
                    CurrentUser = new UserDto
                    {
                        Id = int.TryParse(userId, out int id) ? id : 1,
                        FullName = userName ?? "User",
                        Email = userEmail,
                        Username = userEmail.Split('@')[0], // Extract username from email
                        PhoneNumber = HttpContext.Session.GetString("UserPhone") ?? "",
                        Address = HttpContext.Session.GetString("UserAddress") ?? "",
                        DateOfBirth = DateTime.Now.AddYears(-25), // Default age
                        AvatarUrl = "https://via.placeholder.com/150x150/007bff/ffffff?text=" + (userName?.Substring(0, 1).ToUpper() ?? "U")
                    };
                }
                else
                {
                    // If not logged in, redirect to login
                    Response.Redirect("/Auth/Login");
                }
            }
            catch (Exception ex)
            {
                // Log error and redirect to login
                _logger.LogError(ex, "Error in ViewProfile");
                Response.Redirect("/Auth/Login");
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
