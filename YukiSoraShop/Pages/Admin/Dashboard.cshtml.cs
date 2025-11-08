using Application;
using Application.IRepository;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace YukiSoraShop.Pages.Admin
{
    // Attribute này đã thực hiện việc kiểm tra quyền Admin
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _uow;

        public AdminDashboardModel(
            IUserService userService,
            IProductService productService,
            IOrderService orderService,
            IPaymentService paymentService,
            IUnitOfWork uow)
        {
            _userService = userService;
            _productService = productService;
            _orderService = orderService;
            _paymentService = paymentService;
            _uow = uow;
        }

        public int TotalUsers { get; set; } = 0;
        public int NewUsers { get; set; } = 0;
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public async Task OnGetAsync()
        {
            // Lấy dữ liệu thống kê thực tế
            TotalUsers = await _userService.GetTotalUsersAsync();
            
            // Tính toán NewUsers trực tiếp trong admin page, không thay đổi service
            try
            {
                var dateThreshold = DateTime.UtcNow.AddDays(-30);
                var accounts = await _uow.AccountRepository.GetAllAsync();
                NewUsers = accounts.Count(a => a.CreatedAt >= dateThreshold);
            }
            catch
            {
                NewUsers = 0;
            }
            
            TotalProducts = await _productService.GetTotalProductsAsync();
            TotalOrders = await _orderService.GetTotalOrdersAsync();
            TotalRevenue = await _paymentService.GetTotalRevenueAsync();
        }
    }
}