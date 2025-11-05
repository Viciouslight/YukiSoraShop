using Domain.Entities;

namespace Application.Extensions
{
    public static class ProductQueryableExtensions
    {
        public static IQueryable<Product> FilterBySearch(this IQueryable<Product> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            var s = search.Trim().ToLowerInvariant();
            return query.Where(p => (p.ProductName ?? string.Empty).ToLower().Contains(s));
        }

        public static IQueryable<Product> FilterByCategory(this IQueryable<Product> query, string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return query;
            var c = category.Trim().ToLowerInvariant();
            return query.Where(p => (p.CategoryName ?? string.Empty).ToLower() == c);
        }
    }
}

