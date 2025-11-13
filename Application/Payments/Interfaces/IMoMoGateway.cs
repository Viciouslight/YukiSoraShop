using Application.Payments.DTOs;

namespace Application.Payments.Interfaces
{
    public interface IMoMoGateway
    {
        Task<PaymentCheckoutDTO> GenerateCheckoutUrlAsync(
            int orderId,
            decimal amountVnd,
            string clientIp,
            string? orderDesc,
            CancellationToken ct = default);
    }
}

