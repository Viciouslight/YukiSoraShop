using Application.Payments.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Interfaces
{
    public interface IPaymentGateway
    {
        // Tạo URL thanh toán cho đơn hàng
        Task<PaymentCheckoutDto> CreateCheckoutUrlAsync(CreatePaymentCommand command, CancellationToken ct = default);

        // Parse & validate callback từ VNPay (từ HttpRequest.Query)
        Task<PaymentResultDto> ParseAndValidateCallbackAsync(IQueryCollection query, CancellationToken ct = default);
    }
}
