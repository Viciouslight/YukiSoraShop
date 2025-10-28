using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class CustomerProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<CustomerProfileModel> _logger;

        public CustomerProfileModel(IUserService userService, ILogger<CustomerProfileModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public UserDto? CurrentUser { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var fullName = User.Identity?.Name ?? string.Empty;

                if (!int.TryParse(idStr, out var id) || id <= 0)
                {
                    Response.Redirect("/Auth/Login");
                    return;
                }

                CurrentUser = await _userService.GetUserByIdAsync(id) ?? new UserDto
                {
                    Id = id,
                    FullName = fullName,
                    Email = email,
                    Username = string.IsNullOrEmpty(email) ? fullName : email.Split('@')[0],
                    PhoneNumber = string.Empty,
                    Address = string.Empty,
                    DateOfBirth = DateTime.UtcNow.AddYears(-25),
                    AvatarUrl = "https://via.placeholder.com/150x150/007bff/ffffff?text=" + ((fullName?.Length ?? 0) > 0 ? fullName.Substring(0, 1).ToUpper() : "U")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Profile");
                Response.Redirect("/Auth/Login");
            }
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostLoadUser(int userId)
        {
            return RedirectToPage();
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostSwitchUser(int userId)
        {
            return RedirectToPage();
        }
    }
}
