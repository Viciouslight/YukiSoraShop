using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersCheckoutModel : PageModel
    {
        private readonly IPaymentOrchestrator _payment;
        private readonly IUnitOfWork _uow;

        public OrdersCheckoutModel(IPaymentOrchestrator payment, IUnitOfWork uow)
        {
            _payment = payment;
            _uow = uow;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        [BindProperty]
        public string? BankCode { get; set; }

        [BindProperty]
        public string OrderTypeCode { get; set; } = "other";

        public decimal? GrandTotal { get; set; }

        public async Task<IActionResult> OnGet()
        {
            if (OrderId <= 0)
            {
                TempData["Error"] = "Thi?u OrderId";
                return RedirectToPage("/Customer/Catalog");
            }

            var order = await _uow.OrderRepository.GetByIdAsync(OrderId);
            if (order == null || order.AccountId != GetCurrentUserId())
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (string.Equals(order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (!await IsVnPayActiveAsync())
            {
                TempData["Error"] = "Phương thức VNPay hiện không khả dụng. Vui lòng chọn phương thức khác.";
                return RedirectToPage("/Orders/PaymentMethod", new { OrderId });
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostPayAsync()
        {
            if (OrderId <= 0)
            {
                TempData["Error"] = "Thi?u OrderId";
                return RedirectToPage("/Customer/Catalog");
            }

            var order = await _uow.OrderRepository.GetByIdAsync(OrderId);
            if (order == null || order.AccountId != GetCurrentUserId())
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (string.Equals(order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (!await IsVnPayActiveAsync())
            {
                TempData["Error"] = "Phương thức VNPay hiện không khả dụng. Vui lòng chọn phương thức khác.";
                return RedirectToPage("/Orders/PaymentMethod", new { OrderId });
            }

            var cmd = new CreatePaymentCommand
            {
                OrderId = OrderId,
                BankCode = string.IsNullOrWhiteSpace(BankCode) ? null : BankCode,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                OrderDescription = $"Thanh toan don hang #{OrderId}",
                OrderTypeCode = OrderTypeCode
            };

            var dto = await _payment.CreateCheckoutAsync(cmd);
            return Redirect(dto.CheckoutUrl);
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        private async Task<bool> IsVnPayActiveAsync()
        {
            var method = await _uow.PaymentMethodRepository.FindOneAsync(pm => pm.Name == "VNPay");
            return method?.IsActive ?? false;
        }
    }
}
