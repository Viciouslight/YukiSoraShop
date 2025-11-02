using Application;
using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Domain.Enums;
using Infrastructure.Payments.Providers.VnPay;
using Infrastructure.Payments.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Domain.Entities;

namespace Infrastructure.Payments.Providers
{
    public sealed class PaymentOrchestrator : IPaymentOrchestrator
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IHttpContextAccessor _http;
        private readonly IVnPayGateway _vnPayGateway;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PaymentOrchestrator> _logger;

        public PaymentOrchestrator(IHttpContextAccessor http, IVnPayGateway vnPayGateway, IUnitOfWork uow, IInvoiceService invoiceService, ILogger<PaymentOrchestrator> logger)
        {
            _http = http;
            _vnPayGateway = vnPayGateway;
            _uow = uow;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        public async Task<PaymentCheckoutDTO> CreateCheckoutAsync(CreatePaymentCommand command, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating VNPay checkout for order {OrderId}", command.OrderId);
            var order = await _uow.OrderRepository.GetByIdAsync(command.OrderId);
            if (order is null) throw new InvalidOperationException("Order not found.");
            if (order.GrandTotal is null) order.GrandTotal = order.Subtotal + order.ShippingFee;

            var amountVnd = order.GrandTotal!.Value;

            var method = await EnsurePaymentMethodAsync("VNPay", "VNPay payment gateway", ct);
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethodId = method.Id,
                Amount = amountVnd,
                Currency = "VND",
                PaymentStatus = PaymentStatus.Pending,
            };
            await _uow.PaymentRepository.AddAsync(payment);

            var ip = _http.HttpContext is null ? command.ClientIp : _http.HttpContext.Connection.RemoteIpAddress?.ToString() ?? command.ClientIp;

            var checkout = await _vnPayGateway.GenerateCheckoutUrlAsync(
                orderId: order.Id,
                amountVnd: amountVnd,
                clientIp: ip,
                bankCode: command.BankCode,
                orderDesc: command.OrderDescription,
                orderTypeCode: command.OrderTypeCode,
                ct: ct
            );

            await _uow.SaveChangesAsync();
            _logger.LogInformation("Generated checkout URL for order {OrderId}", order.Id);
            return checkout;
        }

        public async Task<PaymentResultDTO> HandleCallbackAsync(IQueryCollection query, CancellationToken ct = default)
        {
            var result = await _vnPayGateway.ParseAndValidateCallbackAsync(query, ct);
            _logger.LogInformation("Received VNPay callback for order {OrderId}, success={Success}", result.OrderId, result.IsSuccess);

            var order = await _uow.OrderRepository.GetByIdAsync(result.OrderId);
            if (order is null) return result;
            var tx = await _uow.BeginTransactionAsync();
            try
            {

            var payment = await _uow.PaymentRepository.FindOneAsync(
                p => p.OrderId == order.Id && p.PaymentStatus == PaymentStatus.Pending, includeProperties: "PaymentMethod");

            if (payment is null)
            {
                var method = await EnsurePaymentMethodAsync("VNPay", "VNPay payment gateway", ct);
                payment = new Payment
                {
                    OrderId = order.Id,
                    PaymentMethodId = method.Id,
                    Amount = result.Amount,
                    Currency = result.Currency,
                    PaymentStatus = PaymentStatus.Pending,
                };
                await _uow.PaymentRepository.AddAsync(payment);
            }

            payment.PaymentStatus = result.Status;
            payment.TransactionRef = result.TransactionRef;
            payment.RawCallbackJson = result.RawQuery;

            if (result.IsSuccess)
            {
                order.Status = "Paid";
                await _uow.SaveChangesAsync();
                _logger.LogInformation("Payment succeeded for order {OrderId}", result.OrderId);

                // 👉 Tạo invoice ngay sau khi thanh toán thành công (idempotent)
                await _invoiceService.CreateInvoiceFromOrder(order.Id, ct);
                _logger.LogInformation("Order {OrderId} marked as paid and invoice created", order.Id);
            }
            else
            {
                await _uow.SaveChangesAsync();
                _logger.LogWarning("Payment failed for order {OrderId}: {Message}", result.OrderId, result.Message);
            }

                await tx.CommitAsync(ct);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling VNPay callback for order {OrderId}", result.OrderId);
                try { await tx.RollbackAsync(ct); } catch { }
                return result;
            }
        }

        public async Task<PaymentResultDTO> CreateCashPaymentAsync(int orderId, string createdBy, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating cash payment for order {OrderId}", orderId);

            var order = await _uow.OrderRepository.GetByIdAsync(orderId);
            if (order is null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            if (order.GrandTotal is null)
            {
                order.GrandTotal = order.Subtotal + order.ShippingFee;
            }

            var amount = order.GrandTotal!.Value;

            var method = await EnsurePaymentMethodAsync("Cash", "Thanh toán tiền mặt", ct);

            var payment = await _uow.PaymentRepository.FindOneAsync(p => p.OrderId == order.Id && p.PaymentMethodId == method.Id);
            if (payment is null)
            {
                payment = new Payment
                {
                    OrderId = order.Id,
                    PaymentMethodId = method.Id,
                    Amount = amount,
                    Currency = "VND",
                    PaymentStatus = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = createdBy,
                    IsDeleted = false
                };
                await _uow.PaymentRepository.AddAsync(payment);
            }
            else
            {
                payment.Amount = amount;
                payment.Currency = "VND";
                payment.PaymentStatus = PaymentStatus.Pending;
                payment.ModifiedAt = DateTime.UtcNow;
                payment.ModifiedBy = createdBy;
            }

            order.Status = "AwaitingCash";
            order.ModifiedAt = DateTime.UtcNow;
            order.ModifiedBy = createdBy;

            await _uow.SaveChangesAsync();

            return new PaymentResultDTO
            {
                IsSuccess = true,
                Message = "Đơn hàng sẽ được thanh toán bằng tiền mặt khi nhận hàng.",
                OrderId = order.Id,
                Amount = amount,
                Currency = "VND",
                Status = PaymentStatus.Pending
            };
        }

        private async Task<PaymentMethod> EnsurePaymentMethodAsync(string name, string description, CancellationToken ct)
        {
            var pm = await _uow.PaymentMethodRepository.FindOneAsync(p => p.Name == name);
            if (pm is null)
            {
                pm = new PaymentMethod { Name = name, IsActive = true, Description = description };
                await _uow.PaymentMethodRepository.AddAsync(pm);
                await _uow.SaveChangesAsync();
            }
            return pm;
        }
    }
}


