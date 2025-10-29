using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        private readonly IUnitOfWork _uow;

        public AdminDashboardModel(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public int TotalUsers { get; set; } = 3; // Số tài khoản đã seed
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;

        public async Task OnGetAsync()
        {
            // [Authorize] attribute ensures only Admin can access
            // Tính toán thống kê cơ bản từ repositories
            TotalUsers = await _uow.AccountRepository.GetAllQueryable().AsNoTracking().CountAsync();
            TotalProducts = await _uow.ProductRepository.GetAllQueryable().AsNoTracking().CountAsync();
            TotalOrders = await _uow.OrderRepository.GetAllQueryable().AsNoTracking().CountAsync();

            // Tổng doanh thu dựa trên Payments đã thanh toán
            var paidPayments = _uow.PaymentRepository.GetAllQueryable()
                .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Paid);
            TotalRevenue = await paidPayments.SumAsync(p => (decimal?)p.Amount) ?? 0m;
        }
    }
}
