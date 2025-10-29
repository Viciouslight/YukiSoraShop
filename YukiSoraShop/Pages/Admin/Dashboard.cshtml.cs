using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        private readonly IAuthorizationService _authService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        // Cập nhật Constructor
        public AdminDashboardModel(
            IAuthorizationService authService,
            IUserService userService,
            IProductService productService,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            _authService = authService;
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public int TotalUsers { get; set; } = 3; // Số tài khoản đã seed
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public async Task OnGetAsync()
        {
            // Kiểm tra quyền Admin
            if (!_authService.IsAdmin())
            {
                Response.Redirect("/Auth/Login");
                return;
            }

            TotalUsers = await _userService.GetTotalUsersAsync();
            TotalProducts = await _productService.GetTotalProductsAsync();
            TotalOrders = await _orderService.GetTotalOrdersAsync();
            TotalRevenue = await _paymentService.GetTotalRevenueAsync();
            // TODO: Lấy dữ liệu thống kê thực tế từ database
            // Hiện tại sử dụng dữ liệu mẫu
        }
    }
}
