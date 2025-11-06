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
        public List<ProductDTO> RelatedProducts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductDtoByIdAsync(id);
            if (product == null)
                return NotFound();

            Product = product;
            ProductDetails = product.ProductDetails ?? new List<ProductDetailDTO>();

            // Lấy danh sách sản phẩm liên quan cùng danh mục
            if (!string.IsNullOrEmpty(Product.Category))
            {
                var related = await _productService.GetProductsByCategoryAsync(Product.Category);
                RelatedProducts = related
                    .Where(p => p.Id != Product.Id)
                    .Take(4)
                    .ToList();
            }

            return Page();
        }


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCartAsync(int id, int productDetailId)
        {
            // 🧩 Kiểm tra người dùng đã đăng nhập chưa
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Auth/Login");
            }

            // 🧩 Kiểm tra biến thể được chọn chưa
            if (productDetailId <= 0)
            {
                TempData["Error"] = "Vui lòng chọn màu sắc và kích thước trước khi thêm vào giỏ hàng.";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }

            try
            {
                // ✅ Kiểm tra lại biến thể có thực sự thuộc về sản phẩm này không
                var product = await _productService.GetProductDtoByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToPage("/Customer/Catalog");
                }

                var validDetail = product.ProductDetails?.FirstOrDefault(d => d.Id == productDetailId);
                if (validDetail == null)
                {
                    TempData["Error"] = "Biến thể sản phẩm không hợp lệ.";
                    return RedirectToPage("/Customer/ProductDetails", new { id });
                }

                // 🧩 Ghi log debug (hữu ích để kiểm tra)
                Console.WriteLine($"[ADD TO CART] User={accountId} ProductId={id} ProductDetailId={productDetailId}");

                // ✅ Gửi đúng ProductDetailId sang CartService
                await _cartService.AddItemAsync(accountId, productDetailId, 1);

                TempData["Success"] = $"Đã thêm sản phẩm '{product.Name}' ({validDetail.Color}/{validDetail.Size}) vào giỏ hàng!";
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
