using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface ICartService
    {
        // Session/in-memory helpers (for compatibility with existing callers)
        List<CartItemDto> IncreaseQuantity(List<CartItemDto> cart, int productId);
        List<CartItemDto> DecreaseQuantity(List<CartItemDto> cart, int productId);
        List<CartItemDto> RemoveItem(List<CartItemDto> cart, int productId);
        decimal GetTotal(List<CartItemDto> cart);
        int GetCount(List<CartItemDto> cart);
        List<OrderItemInput> ToOrderItems(List<CartItemDto> cart);

        // Persistent cart (Unit of Work + Entities)
        Task<Cart> GetOrCreateCartAsync(int accountId, CancellationToken ct = default);
        Task<IReadOnlyList<CartItem>> GetItemsAsync(int accountId, CancellationToken ct = default);
        Task AddItemAsync(int accountId, int productId, int quantity = 1, CancellationToken ct = default);
        Task UpdateQuantityAsync(int accountId, int productId, int quantity, CancellationToken ct = default);
        Task RemoveItemAsync(int accountId, int productId, CancellationToken ct = default);
        Task ClearAsync(int accountId, CancellationToken ct = default);
        Task<decimal> GetTotalAsync(int accountId, CancellationToken ct = default);
        Task<List<OrderItemInput>> ToOrderItemsAsync(int accountId, CancellationToken ct = default);
    }
}
