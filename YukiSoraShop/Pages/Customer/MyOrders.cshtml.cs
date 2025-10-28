using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class CustomerOrdersModel : PageModel
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CustomerOrdersModel> _logger;

        public CustomerOrdersModel(IUnitOfWork uow, ILogger<CustomerOrdersModel> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public List<OrderItemVm> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var idStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId) || userId <= 0) return RedirectToPage("/Auth/Login");

            try
            {
                var query = _uow.OrderRepository
                    .GetAllQueryable("Payments,Invoices")
                    .Where(o => o.AccountId == userId)
                    .OrderByDescending(o => o.CreatedAt);

                var orders = await query.ToListAsync();

                Orders = orders.Select(o => new OrderItemVm
                {
                    OrderId = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    ShippingFee = o.ShippingFee,
                    GrandTotal = o.GrandTotal ?? o.Subtotal + o.ShippingFee,
                    PaymentStatus = o.Payments.OrderByDescending(p => p.Id).FirstOrDefault()?.PaymentStatus.ToString() ?? "N/A",
                    InvoiceCount = o.Invoices.Count
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for user {UserId}", userId);
                Orders = new List<OrderItemVm>();
            }

            return Page();
        }

        public class OrderItemVm
        {
            public int OrderId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal ShippingFee { get; set; }
            public decimal GrandTotal { get; set; }
            public string PaymentStatus { get; set; } = string.Empty;
            public int InvoiceCount { get; set; }
        }
    }
}

