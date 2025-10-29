using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Application;
using Application.Services.Interfaces;
using Application.Payments.Interfaces;

namespace YukiSoraShop.Pages.Admin
{
    // Attribute này đã thực hiện việc kiểm tra quyền Admin
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPaymentQueryService _paymentQuery;

        // Inject services for totals, UoW for revenue
        public AdminDashboardModel(IUnitOfWork uow, IUserService userService, IProductService productService, IOrderService orderService, IPaymentQueryService paymentQuery)
        {
            _uow = uow;
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
            _paymentQuery = paymentQuery;
        }

        public int TotalUsers { get; set; } = 0; // Đổi lại giá trị mặc định là 0
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public async Task OnGetAsync()
        {
            // Lấy dữ liệu thống kê thông qua service layer
            TotalUsers = await _userService.GetTotalUsersAsync();
            TotalProducts = await _productService.GetTotalProductsAsync();
            TotalOrders = await _orderService.GetTotalOrdersAsync();

            TotalRevenue = await _paymentQuery.GetTotalRevenueAsync();
        }
    }
}
