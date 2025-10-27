using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Orders
{
    public class PayModel : PageModel
    {
        private readonly IPaymentOrchestrator _payment;
        private readonly IUnitOfWork _uow;

        public PayModel(IPaymentOrchestrator payment, IUnitOfWork uow)
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
                return RedirectToPage("/Index");
            }

            var order = await _uow.OrderRepository.GetByIdAsync(OrderId);
            GrandTotal = order?.GrandTotal ?? ((order?.Subtotal ?? 0) + (order?.ShippingFee ?? 0));

            return Page();
        }

        public async Task<IActionResult> OnPostPayAsync()
        {
            if (OrderId <= 0)
            {
                TempData["Error"] = "Thi?u OrderId";
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
            return Redirect(dto.CheckoutUrl);
        }
    }
}
