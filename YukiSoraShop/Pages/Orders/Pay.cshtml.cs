using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Orders
{
    public class PayModel : PageModel
    {
        private readonly IPaymentOrchestrator _payment;

        public PayModel(IPaymentOrchestrator payment)
        {
            _payment = payment;
        }

        // Giả sử bạn truyền OrderId qua query ?orderId=...
        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        // Người dùng chọn mã BankCode (tùy chọn)
        [BindProperty]
        public string? BankCode { get; set; }

        // Người dùng chọn OrderType theo catalog VNPay (bắt buộc vnp_OrderType)
        [BindProperty]
        public string OrderTypeCode { get; set; } = "other"; // đổi thành mã chuẩn của bạn

        // Hiển thị thông tin đơn hàng cơ bản (tùy bạn load thêm)
        public decimal? GrandTotal { get; set; }

        public IActionResult OnGet()
        {
            if (OrderId <= 0)
            {
                TempData["Error"] = "Thiếu OrderId";
                return RedirectToPage("/Index");
            }

            // TODO: load Order từ DB để hiển thị (demo đặt cứng)
            // Bạn có thể inject IUnitOfWork để lấy ra tổng tiền
            GrandTotal = null; // ví dụ: set từ order.GrandTotal

            return Page();
        }

        public async Task<IActionResult> OnPostPayAsync()
        {
            if (OrderId <= 0)
            {
                TempData["Error"] = "Thiếu OrderId";
                return RedirectToPage("/Index");
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

            // Redirect người dùng sang trang thanh toán VNPay
            return Redirect(dto.CheckoutUrl);
        }
    }
}