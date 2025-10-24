using YukiSoraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IAuthorizationService _authService;

        public DashboardModel(IAuthorizationService authService)
        {
            _authService = authService;
        }

        public int TotalUsers { get; set; } = 3; // Số tài khoản đã seed
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public void OnGet()
        {
            // Kiểm tra quyền Admin
            if (!_authService.IsAdmin())
            {
                Response.Redirect("/Auth/Login");
                return;
            }

            // TODO: Lấy dữ liệu thống kê thực tế từ database
            // Hiện tại sử dụng dữ liệu mẫu
        }
    }
}
