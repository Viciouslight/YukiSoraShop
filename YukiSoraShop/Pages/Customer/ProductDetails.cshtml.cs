using Application.DTOs;
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

        public ProductDTO Product { get; set; } = new();

        public ProductDetailsModel(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductDtoByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToPage("/Customer/Catalog");
            }

            Product = product;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCartAsync(int id, int productDetailId)
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var product = await _productService.GetProductDtoByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToPage("/Customer/Catalog");
                }

                ProductDetailDTO? selectedDetail = null;
                if (productDetailId > 0)
                    selectedDetail = product.ProductDetails?.FirstOrDefault(d => d.Id == productDetailId);

                if (productDetailId > 0 && selectedDetail == null)
                {
                    TempData["Error"] = "Biến thể sản phẩm không tồn tại.";
                    return RedirectToPage("/Customer/ProductDetails", new { id });
                }

                decimal finalPrice = product.Price + (selectedDetail?.AdditionalPrice ?? 0);
               // await _cartService.AddItemAsync(accountId, id, 1, productDetailId, finalPrice);

                TempData["Success"] = $"Đã thêm {product.Name} {(selectedDetail != null ? $"({selectedDetail.Color} - {selectedDetail.Size})" : "")} vào giỏ hàng!";
                return RedirectToPage("/Customer/Cart");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToCart ERROR] {ex}");
                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }
        }
    }
}
