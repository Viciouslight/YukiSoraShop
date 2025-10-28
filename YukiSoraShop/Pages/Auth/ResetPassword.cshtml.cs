using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace YukiSoraShop.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IUserService _userService;

        public ResetPasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(string email)
        {
            // Pre-fill email if provided
            if (!string.IsNullOrEmpty(email))
            {
                Input.Email = email;
            }
            else
            {
                // Try to get email from session
                var sessionEmail = HttpContext.Session.GetString("ResetEmail");
                if (!string.IsNullOrEmpty(sessionEmail))
                {
                    Input.Email = sessionEmail;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Get reset token from session
                var sessionToken = HttpContext.Session.GetString("ResetToken");
                var sessionEmail = HttpContext.Session.GetString("ResetEmail");
                var sessionExpiry = HttpContext.Session.GetString("ResetExpiry");

                // Check if reset token exists and is valid
                if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(sessionEmail))
                {
                    ErrorMessage = "âŒ Reset token khÃ´ng há»£p lá»‡ hoáº·c Ä‘Ã£ háº¿t háº¡n. Vui lÃ²ng thá»­ láº¡i.";
                    return Page();
                }

                // Check if token matches (case insensitive)
                if (string.IsNullOrEmpty(Input.ResetToken) || 
                    !string.Equals(Input.ResetToken.Trim(), sessionToken, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = $"âŒ MÃ£ reset khÃ´ng Ä‘Ãºng. MÃ£ báº¡n nháº­p: '{Input.ResetToken}', MÃ£ Ä‘Ãºng: '{sessionToken}'";
                    return Page();
                }

                // Check if email matches
                if (Input.Email != sessionEmail)
                {
                    ErrorMessage = "âŒ Email khÃ´ng khá»›p vá»›i yÃªu cáº§u reset.";
                    return Page();
                }

                // Check expiry (simple check)
                if (DateTime.TryParse(sessionExpiry, out var expiry) && DateTime.Now > expiry)
                {
                    ErrorMessage = "âŒ MÃ£ reset Ä‘Ã£ háº¿t háº¡n. Vui lÃ²ng yÃªu cáº§u mÃ£ má»›i.";
                    return Page();
                }

                // Validate new password
                if (Input.NewPassword != Input.ConfirmPassword)
                {
                    ErrorMessage = "âŒ Máº­t kháº©u má»›i vÃ  xÃ¡c nháº­n khÃ´ng khá»›p.";
                    return Page();
                }

                if (Input.NewPassword.Length < 6)
                {
                    ErrorMessage = "âŒ Máº­t kháº©u má»›i pháº£i cÃ³ Ã­t nháº¥t 6 kÃ½ tá»±.";
                    return Page();
                }

                // Update password in database
                var success = await _userService.ChangePasswordAsync(Input.Email, Input.NewPassword);
                if (success)
                {
                    // Clear session data
                    HttpContext.Session.Remove("ResetToken");
                    HttpContext.Session.Remove("ResetEmail");
                    HttpContext.Session.Remove("ResetExpiry");

                    SuccessMessage = "ðŸŽ‰ Máº­t kháº©u Ä‘Ã£ Ä‘Æ°á»£c Ä‘áº·t láº¡i thÃ nh cÃ´ng! Báº¡n cÃ³ thá»ƒ Ä‘Äƒng nháº­p vá»›i máº­t kháº©u má»›i.";
                    TempData["SuccessMessage"] = "ðŸŽ‰ Máº­t kháº©u Ä‘Ã£ Ä‘Æ°á»£c Ä‘áº·t láº¡i thÃ nh cÃ´ng! Báº¡n cÃ³ thá»ƒ Ä‘Äƒng nháº­p vá»›i máº­t kháº©u má»›i.";
                    TempData["ShowSuccess"] = "true";
                    return RedirectToPage("./Login");
                }
                else
                {
                    ErrorMessage = "âŒ KhÃ´ng thá»ƒ Ä‘áº·t láº¡i máº­t kháº©u. Vui lÃ²ng thá»­ láº¡i.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "âŒ CÃ³ lá»—i xáº£y ra. Vui lÃ²ng thá»­ láº¡i sau.";
                
                return Page();
            }
        }
    }

    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reset code is required")]
        [Display(Name = "Reset Code")]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

