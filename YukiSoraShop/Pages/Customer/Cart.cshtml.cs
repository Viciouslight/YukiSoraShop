using Application.Services.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Linq;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class CustomerCartModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public CustomerCartModel(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }

        public async Task OnGetAsync()
        {
            await LoadFromPersistentCartAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, string action)
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0) return RedirectToPage("/Auth/Login");

            var items = await _cartService.GetItemsAsync(accountId);
            var item = items.FirstOrDefault(i => i.ProductId == productId);
            var currentQty = item?.Quantity ?? 0;
            var newQty = action == "increase" ? currentQty + 1 : Math.Max(1, currentQty - 1);
            if (currentQty == 0 && action == "decrease") newQty = 0;

            await _cartService.UpdateQuantityAsync(accountId, productId, newQty);
            await LoadFromPersistentCartAsync();
            return RedirectToPage();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostRemoveItemAsync(int productId)
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0) return RedirectToPage("/Auth/Login");
            await _cartService.RemoveItemAsync(accountId, productId);
            await LoadFromPersistentCartAsync();
            return RedirectToPage();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCheckout()
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0) return RedirectToPage("/Auth/Login");

            var createdBy = User.Identity?.Name ?? "customer";
            var orderItems = await _cartService.ToOrderItemsAsync(accountId);
            if (orderItems == null || orderItems.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage();
            }

            var order = await _orderService.CreateOrderFromCartAsync(accountId, orderItems, createdBy);
            await _cartService.ClearAsync(accountId);
            await LoadFromPersistentCartAsync();
            return Redirect($"/Orders/Checkout?OrderId={order.Id}");
        }

        private async Task LoadFromPersistentCartAsync()
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0)
            {
                CartItems = new List<CartItemDto>();
                TotalAmount = 0;
                TotalItems = 0;
                return;
            }
            var items = await _cartService.GetItemsAsync(accountId);
            CartItems = items.Select(i => new CartItemDto
            {
                Quantity = i.Quantity,
                Product = new ProductDto
                {
                    Id = i.ProductId,
                    Name = i.Product?.ProductName ?? $"Product #{i.ProductId}",
                    Description = i.Product?.Description ?? string.Empty,
                    Price = i.Product?.Price ?? 0m,
                    ImageUrl = i.Product?.ProductDetails.FirstOrDefault()?.ImageUrl ?? string.Empty,
                    Category = i.Product?.CategoryName ?? string.Empty,
                    Stock = i.Product?.StockQuantity ?? 0,
                    IsAvailable = !(i.Product?.IsDeleted ?? false)
                }
            }).ToList();
            TotalAmount = CartItems.Sum(ci => ci.TotalPrice);
            TotalItems = CartItems.Sum(ci => ci.Quantity);
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }
    }
}
