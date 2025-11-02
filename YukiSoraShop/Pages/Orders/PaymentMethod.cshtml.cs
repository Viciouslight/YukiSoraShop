using Application.Payments.Interfaces;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersPaymentMethodModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentOrchestrator _paymentOrchestrator;

        public OrdersPaymentMethodModel(IOrderService orderService, IPaymentOrchestrator paymentOrchestrator)
        {
            _orderService = orderService;
            _paymentOrchestrator = paymentOrchestrator;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        [BindProperty]
        public string SelectedMethod { get; set; } = "VNPay";

        public decimal GrandTotal { get; set; }
        public bool IsAwaitingCash { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var order = await LoadOrderAsync();
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Customer/Catalog");
            }

            if (string.Equals(order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
            IsAwaitingCash = string.Equals(order.Status, "AwaitingCash", StringComparison.OrdinalIgnoreCase);
            if (IsAwaitingCash)
            {
                SelectedMethod = "Cash";
            }
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var order = await LoadOrderAsync();
                if (order != null)
                {
                    GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
                    IsAwaitingCash = string.Equals(order.Status, "AwaitingCash", StringComparison.OrdinalIgnoreCase);
                }
                return Page();
            }

            var currentOrder = await LoadOrderAsync();
            if (currentOrder == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Customer/Catalog");
            }
            IsAwaitingCash = string.Equals(currentOrder.Status, "AwaitingCash", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(SelectedMethod, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                var createdBy = User.Identity?.Name ?? "customer";
                var result = await _paymentOrchestrator.CreateCashPaymentAsync(OrderId, createdBy);
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage("/Orders/CashConfirmation", new { OrderId });
            }

            return RedirectToPage("/Orders/Checkout", new { OrderId });
        }

        private async Task<Domain.Entities.Order?> LoadOrderAsync()
        {
            var order = await _orderService.GetOrderWithDetailsAsync(OrderId);
            if (order == null)
            {
                return null;
            }

            var userId = GetCurrentUserId();
            if (order.AccountId != userId)
            {
                return null;
            }

            return order;
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }
    }
}
