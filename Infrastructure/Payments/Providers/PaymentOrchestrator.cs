using Application;
using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Payments.Providers.VnPay;
using Infrastructure.Payments.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Payments.Providers
{
    public sealed class PaymentOrchestrator : IPaymentOrchestrator
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IHttpContextAccessor _http;
        private readonly IPaymentGateway _gateway; // VnPayGateway
        private readonly IUnitOfWork _uow;

        public PaymentOrchestrator(IHttpContextAccessor http, IPaymentGateway gateway, IUnitOfWork uow)
        {
            _http = http;
            _gateway = gateway;
            _uow = uow;
        }

        public async Task<PaymentCheckoutDto> CreateCheckoutAsync(CreatePaymentCommand command, CancellationToken ct = default)
        {
            var order = await _uow.OrderRepository.GetByIdAsync(command.OrderId);
            if (order is null) throw new InvalidOperationException("Order not found.");
            if (order.GrandTotal is null) order.GrandTotal = order.Subtotal + order.ShippingFee;

            var amountVnd = order.GrandTotal!.Value;

            var method = await EnsureVnPayMethodAsync(ct);
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

            var vnGateway = _gateway as VnPayGateway
                ?? throw new InvalidOperationException("VNPay gateway required.");

            var checkout = await vnGateway.CreateCheckoutUrlInternalAsync(
                orderId: order.Id,
                amountVnd: amountVnd,
                clientIp: ip,
                bankCode: command.BankCode,
                orderDesc: command.OrderDescription,
                orderTypeCode: command.OrderTypeCode,
                ct: ct
            );

            await _uow.SaveChangesAsync();
            return checkout;
        }

        public async Task<PaymentResultDto> HandleCallbackAsync(IQueryCollection query, CancellationToken ct = default)
        {
            var result = await _gateway.ParseAndValidateCallbackAsync(query, ct);

            var order = await _uow.OrderRepository.GetByIdAsync(result.OrderId);
            if (order is null) return result;

            var payment = await _uow.PaymentRepository.FindOneAsync(
                p => p.OrderId == order.Id && p.PaymentStatus == PaymentStatus.Pending, includeProperties: "PaymentMethod");

            if (payment is null)
            {
                var method = await EnsureVnPayMethodAsync(ct);
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

                // 👉 Tạo invoice ngay sau khi thanh toán thành công (idempotent)
                await _invoiceService.CreateInvoiceFromOrder(order.Id, ct);
            }
            else
            {
                await _uow.SaveChangesAsync();
            }

            return result;
        }

        private async Task<PaymentMethod> EnsureVnPayMethodAsync(CancellationToken ct)
        {
            var pm = await _uow.PaymentMethodRepository.FindOneAsync(p => p.Name == "VNPay");
            if (pm is null)
            {
                pm = new PaymentMethod { Name = "VNPay", IsActive = true, Description = "VNPay payment gateway" };
                await _uow.PaymentMethodRepository.AddAsync(pm);
                await _uow.SaveChangesAsync();
            }
            return pm;
        }
    }
}
