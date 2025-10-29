using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Domain.Entities;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator")]
    public class StaffProductDeleteModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductDeleteModel> _logger;

        public StaffProductDeleteModel(IProductService productService, ILogger<StaffProductDeleteModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public Product? Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Product = await _productService.GetProductEntityByIdAsync(id);
            if (Product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToPage("/Staff/Products/List");
            }
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (Product == null) return RedirectToPage("/Staff/Products/List");
            try
            {
                var ok = await _productService.DeleteProductAsync(Product.Id);
                if (ok)
                {
                    TempData["SuccessMessage"] = "Xoá sản phẩm thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xoá sản phẩm.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", Product?.Id);
                TempData["Error"] = "Có lỗi xảy ra khi xoá sản phẩm.";
            }

            return RedirectToPage("/Staff/Products/List");
        }
    }
}

