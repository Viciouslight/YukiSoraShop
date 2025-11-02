using Application.Admin.Interfaces;
using Application.Payments.Interfaces;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using YukiSoraShop.Hubs;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersPaymentMethodModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentOrchestrator _paymentOrchestrator;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IHubContext<AdminDashboardHub> _hubContext;
        private readonly ILogger<OrdersPaymentMethodModel> _logger;

        public OrdersPaymentMethodModel(
            IOrderService orderService,
            IPaymentOrchestrator paymentOrchestrator,
            IAdminDashboardService dashboardService,
            IHubContext<AdminDashboardHub> hubContext,
            ILogger<OrdersPaymentMethodModel> logger)
        {
            _orderService = orderService;
            _paymentOrchestrator = paymentOrchestrator;
            _dashboardService = dashboardService;
            _hubContext = hubContext;
            _logger = logger;
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
                if (result.IsSuccess)
                {
                    try
                    {
                        var orderSnapshot = await _orderService.GetOrderWithDetailsAsync(OrderId);
                        if (orderSnapshot != null)
                        {
                            await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                                .SendAsync("CashOrderAdded", new
                                {
                                    orderId = orderSnapshot.Id,
                                    customerName = orderSnapshot.Account?.FullName ?? orderSnapshot.Account?.UserName ?? $"User #{orderSnapshot.AccountId}",
                                    customerEmail = orderSnapshot.Account?.Email ?? string.Empty,
                                    createdAt = orderSnapshot.CreatedAt,
                                    grandTotal = orderSnapshot.GrandTotal ?? (orderSnapshot.Subtotal + orderSnapshot.ShippingFee),
                                    paymentStatus = "Pending"
                                });
                        }

                        var summary = await _dashboardService.RefreshSummaryAsync();
                        await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                            .SendAsync("DashboardSummaryUpdated", summary);
                    }
                    catch (Exception broadcastEx)
                    {
                        _logger.LogWarning(broadcastEx, "Failed to broadcast cash order creation for OrderId {OrderId}", OrderId);
                    }
                }
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
