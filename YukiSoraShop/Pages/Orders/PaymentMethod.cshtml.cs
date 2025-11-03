using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Admin.Interfaces;
using Application.Payments.Interfaces;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using YukiSoraShop.Hubs;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersPaymentMethodModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentOrchestrator _paymentOrchestrator;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IHubContext<AdminDashboardHub> _hubContext;
        private readonly ILogger<OrdersPaymentMethodModel> _logger;

        public OrdersPaymentMethodModel(
            IOrderService orderService,
            IPaymentOrchestrator paymentOrchestrator,
            IPaymentMethodService paymentMethodService,
            IAdminDashboardService dashboardService,
            IHubContext<AdminDashboardHub> hubContext,
            ILogger<OrdersPaymentMethodModel> logger)
        {
            _orderService = orderService;
            _paymentOrchestrator = paymentOrchestrator;
            _paymentMethodService = paymentMethodService;
            _dashboardService = dashboardService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        [BindProperty]
        public string SelectedMethod { get; set; } = string.Empty;

        public decimal GrandTotal { get; set; }
        public bool IsAwaitingCash { get; set; }
        public List<PaymentMethodOption> PaymentMethods { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var order = await LoadOrderAsync();
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Customer/Catalog");
            }

            if (EqualsName(order.Status, "Paid"))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
            IsAwaitingCash = EqualsName(order.Status, "AwaitingCash");

            if (!await LoadPaymentMethodsAsync())
            {
                TempData["Error"] = "Hiện chưa có phương thức thanh toán nào khả dụng.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (IsAwaitingCash && HasMethod("Cash"))
            {
                SelectedMethod = "Cash";
            }
            else if (string.IsNullOrWhiteSpace(SelectedMethod) || !HasMethod(SelectedMethod))
            {
                SelectedMethod = PaymentMethods.First().Name;
            }

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            var order = await LoadOrderAsync();
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Customer/Catalog");
            }

            if (EqualsName(order.Status, "Paid"))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
            IsAwaitingCash = EqualsName(order.Status, "AwaitingCash");

            if (!await LoadPaymentMethodsAsync())
            {
                TempData["Error"] = "Hiện chưa có phương thức thanh toán nào khả dụng.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (!HasMethod(SelectedMethod))
            {
                ModelState.AddModelError(nameof(SelectedMethod), "Phương thức thanh toán không khả dụng.");
                SelectedMethod = PaymentMethods.First().Name;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (EqualsName(SelectedMethod, "Cash"))
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

            if (!EqualsName(SelectedMethod, "VNPay"))
            {
                TempData["Error"] = "Phương thức thanh toán chưa được hỗ trợ.";
                return Page();
            }

            return RedirectToPage("/Orders/Checkout", new { OrderId });
        }

        private async Task<bool> LoadPaymentMethodsAsync()
        {
            var methods = await _paymentMethodService.GetActiveAsync();
            PaymentMethods = methods
                .Select(pm => new PaymentMethodOption
                {
                    Id = pm.Id,
                    Name = pm.Name,
                    Description = pm.Description ?? string.Empty
                })
                .ToList();
            return PaymentMethods.Count > 0;
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

        private bool HasMethod(string? name) =>
            !string.IsNullOrWhiteSpace(name) && PaymentMethods.Any(pm => EqualsName(pm.Name, name));

        private static bool EqualsName(string? left, string? right) =>
            string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        public class PaymentMethodOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
