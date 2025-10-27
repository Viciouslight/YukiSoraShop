using Application.DTOs;
using Application.Models;

namespace Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(int accountId, IEnumerable<OrderItemInput> items, string createdBy, CancellationToken ct = default);
    }
}

