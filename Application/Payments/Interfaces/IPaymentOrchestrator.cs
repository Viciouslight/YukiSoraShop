using Application.Payments.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Payments.Interfaces
{
    public interface IPaymentOrchestrator
    {
        Task<PaymentCheckoutDTO> CreateCheckoutAsync(CreatePaymentCommand command, CancellationToken ct = default);

        Task<PaymentResultDTO> HandleCallbackAsync(IQueryCollection query, CancellationToken ct = default);
    }
}
