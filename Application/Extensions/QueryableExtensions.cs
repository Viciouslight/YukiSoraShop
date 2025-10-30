using Application.DTOs.Pagination;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<T>
            {
                Items = items,
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static async Task<PagedResult<TDest>> ToPagedResultAsync<TSource, TDest>(this IQueryable<TSource> query, int pageNumber, int pageSize, Expression<Func<TSource, TDest>> selector)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(selector).ToListAsync();
            return new PagedResult<TDest>
            {
                Items = items,
                TotalItems = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Tìm kiếm theo tên sản phẩm
        public static IQueryable<Product> FilterBySearch(this IQueryable<Product> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            return query.Where(p => p.ProductName.Contains(search));
        }

        // Lọc theo tên danh mục
        public static IQueryable<Product> FilterByCategory(this IQueryable<Product> query, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return query;
            return query.Where(p => p.CategoryName == category);
        }
    }
}

