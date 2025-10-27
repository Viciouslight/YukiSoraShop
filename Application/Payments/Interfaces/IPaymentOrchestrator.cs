using Application.Payments.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Payments.Interfaces
{
    public interface IPaymentOrchestrator
    {
        // Dùng ở Razor Page: trả url redirect
        Task<PaymentCheckoutDto> CreateCheckoutAsync(CreatePaymentCommand command, CancellationToken ct = default);

        // Dùng ở endpoint callback (Page/Handler sau này): xử lý + lưu Payment
        Task<PaymentResultDto> HandleCallbackAsync(IQueryCollection query, CancellationToken ct = default);
    }
}
