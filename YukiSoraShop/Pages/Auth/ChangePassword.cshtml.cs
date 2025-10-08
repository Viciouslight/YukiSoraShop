using Application.Models;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace YukiSoraShop.Pages.Auth
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserService _userService;

        public ChangePasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Check if user is logged in
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                Response.Redirect("/Auth/Login");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Check if user is logged in
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToPage("/Auth/Login");
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Verify current password
                var currentUser = await _userService.LoginAsync(userEmail, Input.CurrentPassword);
                if (currentUser == null)
                {
                    ErrorMessage = "Current password is incorrect.";
                    return Page();
                }

                // Validate new password
                if (Input.NewPassword != Input.ConfirmPassword)
                {
                    ErrorMessage = "New password and confirmation do not match.";
                    return Page();
                }

                if (Input.NewPassword.Length < 6)
                {
                    ErrorMessage = "New password must be at least 6 characters long.";
                    return Page();
                }

                // Update password in database
                var success = await _userService.ChangePasswordAsync(userEmail, Input.NewPassword);
                if (success)
                {
                    SuccessMessage = "🎉 Password changed successfully! Your account is now more secure.";
                    TempData["SuccessMessage"] = "🎉 Password changed successfully! Your account is now more secure.";
                    return RedirectToPage("/Customer/ViewProfile");
                }
                else
                {
                    ErrorMessage = "❌ Failed to change password. Please try again.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while changing password. Please try again.";
                Console.WriteLine($"Error changing password: {ex.Message}");
                return Page();
            }
        }
    }

    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
