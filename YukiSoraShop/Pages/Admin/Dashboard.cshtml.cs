using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        public AdminDashboardModel()
        {
        }

        public int TotalUsers { get; set; } = 3; // Số tài khoản đã seed
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public void OnGet()
        {
            // Kiểm tra quyền Admin
            

            // TODO: Lấy dữ liệu thống kê thực tế từ database
            // Hiện tại sử dụng dữ liệu mẫu
        }
    }
}
