using Application.DTOs;
using Application.Services;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    public class ProductDetailsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public ProductDetailsModel(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public ProductDTO Product { get; set; } = default!;
        public List<ProductDetailDTO> ProductDetails { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductDtoByIdAsync(id);
            if (product == null)
                return NotFound();

            Product = product;
            ProductDetails = product.ProductDetails ?? new List<ProductDetailDTO>();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCartAsync(int id)
        {
            // Lấy ID tài khoản từ session đăng nhập
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                await _cartService.AddItemAsync(accountId, id, 1);
                TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.";
                Console.WriteLine($"[AddToCart ERROR] {ex.Message}");
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }
        }

    }
}
