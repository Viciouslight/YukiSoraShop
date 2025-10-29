using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    // Attribute này đã thực hiện việc kiểm tra quyền Admin
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        // Đã xóa: private readonly IAuthorizationService _authService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        // Cập nhật Constructor (Đã xóa IAuthorizationService)
        public AdminDashboardModel(
            IUserService userService,
            IProductService productService,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            // Đã xóa: _authService = authService;
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public int TotalUsers { get; set; } = 0; // Đổi lại giá trị mặc định là 0
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public async Task OnGetAsync()
        {

            // Lấy dữ liệu thống kê thực tế
            TotalUsers = await _userService.GetTotalUsersAsync();
            TotalProducts = await _productService.GetTotalProductsAsync();
            TotalOrders = await _orderService.GetTotalOrdersAsync();
            TotalRevenue = await _paymentService.GetTotalRevenueAsync();
        }
    }
}