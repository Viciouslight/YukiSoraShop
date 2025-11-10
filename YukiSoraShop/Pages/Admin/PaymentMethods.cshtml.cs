using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class PaymentMethodsModel : PageModel
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodsModel> _logger;

        public PaymentMethodsModel(IPaymentMethodService paymentMethodService, ILogger<PaymentMethodsModel> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }

        public List<PaymentMethodVm> Methods { get; private set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        // Remove [ValidateAntiForgeryToken] - Razor Pages handles this automatically
        public async Task<IActionResult> OnPostSetStatusAsync(int id, bool isActive)
        {
            try
            {
                _logger.LogInformation("Attempting to set payment method {Id} to {IsActive}", id, isActive);
                
                var modifiedBy = User?.Identity?.Name ?? "administrator";
                var success = await _paymentMethodService.SetStatusAsync(id, isActive, modifiedBy);
                
                if (success)
                {
                    _logger.LogInformation("Successfully updated payment method {Id} to {IsActive}", id, isActive);
                    StatusMessage = isActive
                        ? "✅ Đã kích hoạt lại phương thức thanh toán."
                        : "✅ Đã tạm ngưng phương thức thanh toán.";
                }
                else
                {
                    _logger.LogWarning("Payment method {Id} not found", id);
                    ErrorMessage = "Không tìm thấy phương thức thanh toán cần cập nhật.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for payment method {Id} to {IsActive}", id, isActive);
                ErrorMessage = "Không thể cập nhật trạng thái. Vui lòng thử lại.";
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            try
            {
                var items = await _paymentMethodService.GetAllAsync();
                Methods = items.Select(pm => new PaymentMethodVm
                {
                    Id = pm.Id,
                    Name = pm.Name,
                    Description = pm.Description ?? string.Empty,
                    IsActive = pm.IsActive,
                    ModifiedAt = pm.ModifiedAt,
                    ModifiedBy = pm.ModifiedBy ?? string.Empty
                }).ToList();
                
                _logger.LogInformation("Loaded {Count} payment methods", Methods.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment methods");
                Methods = new List<PaymentMethodVm>();
            }
        }

        public class PaymentMethodVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime? ModifiedAt { get; set; }
            public string ModifiedBy { get; set; } = string.Empty;
        }
    }
}
