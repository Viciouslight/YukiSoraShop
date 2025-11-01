using Application;
using Application.Admin.DTOs;
using Application.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Reporting
{
    public sealed class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "admin:dashboard:summary";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public AdminDashboardService(IUnitOfWork uow, IMemoryCache cache)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<AdminDashboardSummary> GetSummaryAsync(CancellationToken ct = default)
        {
            if (_cache.TryGetValue(CacheKey, out AdminDashboardSummary cached))
            {
                return cached;
            }

            var totalUsers = await _uow.AccountRepository.GetCountAsync();
            var totalProducts = await _uow.ProductRepository.GetCountAsync();
            var totalOrders = await _uow.OrderRepository.GetCountAsync();

            var revenueQuery = _uow.PaymentRepository.GetAllQueryable()
                .AsNoTracking()
                .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Paid);
            var totalRevenue = await revenueQuery.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var summary = new AdminDashboardSummary
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue
            };

            _cache.Set(CacheKey, summary, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            });

            return summary;
        }
    }
}
