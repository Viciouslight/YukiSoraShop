using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Infrastructure.Payments.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Payments.Providers.VnPay
{
    public sealed class VnPayGateway : IPaymentGateway
    {
        private readonly VnPayOptions _opt;

        public VnPayGateway(IOptions<VnPayOptions> opt)
        {
            _opt = opt.Value;
        }

        public Task<PaymentCheckoutDTO> CreateCheckoutUrlAsync(CreatePaymentCommand command, CancellationToken ct = default)
        {
            throw new NotSupportedException("Use PaymentOrchestrator to generate the checkout URL with validated order amount.");
        }

        internal Task<PaymentCheckoutDTO> CreateCheckoutUrlInternalAsync(
            int orderId, decimal amountVnd, string clientIp, string? bankCode, string? orderDesc, string orderTypeCode, CancellationToken ct)
        {
            var nowUtc = DateTime.UtcNow;
            var createDate = ToGmt7String(nowUtc, _opt.TimeZoneId);
            var expireDate = ToGmt7String(nowUtc.AddMinutes(_opt.ExpireMinutes), _opt.TimeZoneId);
            var orderInfo = SanitizeOrderInfo(orderDesc ?? $"Thanh toan don hang #{orderId}");

            var vnp = new VnPayLibrary();
            var txnRef = $"{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            vnp.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnp.AddRequestData("vnp_Command", _opt.vnp_Command);
            vnp.AddRequestData("vnp_TmnCode", _opt.vnp_TmnCode);
            vnp.AddRequestData("vnp_Amount", ((long)(amountVnd * 100)).ToString()); // VND * 100
            vnp.AddRequestData("vnp_CreateDate", createDate);
            vnp.AddRequestData("vnp_ExpireDate", expireDate);
            vnp.AddRequestData("vnp_CurrCode", _opt.vnp_CurrCode);
            vnp.AddRequestData("vnp_IpAddr", clientIp);
            vnp.AddRequestData("vnp_Locale", _opt.vnp_Locale);
            vnp.AddRequestData("vnp_OrderInfo", orderInfo);
            vnp.AddRequestData("vnp_OrderType", orderTypeCode); 
            vnp.AddRequestData("vnp_ReturnUrl", _opt.vnp_ReturnUrl);
            vnp.AddRequestData("vnp_TxnRef", txnRef);

            if (!string.IsNullOrWhiteSpace(bankCode))
                vnp.AddRequestData("vnp_BankCode", bankCode);

            var url = vnp.CreateRequestUrl(_opt.vnp_BaseUrl, _opt.vnp_HashSecret);

            return Task.FromResult(new PaymentCheckoutDTO
            {
                CheckoutUrl = url,
                Provider = "VNPay"
            });
        }

        public Task<PaymentResultDTO> ParseAndValidateCallbackAsync(IQueryCollection query, CancellationToken ct = default)
        {
            var vnp = new VnPayLibrary();
            var rawDict = QueryHelpers.ParseQuery(query.ToString());
            foreach (var kv in query)
            {
                if (kv.Key.StartsWith("vnp_"))
                    vnp.AddResponseData(kv.Key, kv.Value);
            }

            var inputHash = query["vnp_SecureHash"].ToString();
            var isValid = vnp.ValidateSignature(inputHash, _opt.vnp_HashSecret);

            var rspCode = vnp.GetResponseData("vnp_ResponseCode");          
            var txnStatus = vnp.GetResponseData("vnp_TransactionStatus");   
            var amountStr = vnp.GetResponseData("vnp_Amount");
            var txnRef = vnp.GetResponseData("vnp_TxnRef");
            var bankCode = vnp.GetResponseData("vnp_BankCode");
            var payDate = vnp.GetResponseData("vnp_PayDate");

            var (orderId, _) = ParseOrderId(txnRef);

            var amountVnd = 0m;
            if (long.TryParse(amountStr, out var amt))
                amountVnd = amt / 100m;

            var ok = isValid && rspCode == "00" && txnStatus == "00";

            var result = new PaymentResultDTO
            {
                IsSuccess = ok,
                Message = ok ? "Payment successful" : $"Payment failed (code={rspCode}, status={txnStatus}, valid={isValid})",
                TransactionRef = txnRef,
                BankCode = bankCode,
                PayDate = payDate,
                RawQuery = query.ToString(),
                Amount = amountVnd,
                Currency = "VND",
                Status = ok ? Domain.Enums.PaymentStatus.Paid : Domain.Enums.PaymentStatus.Canceled,
                OrderId = orderId
            };
            return Task.FromResult(result);
        }

        private static (int orderId, long ts) ParseOrderId(string txnRef)
        {
            var parts = (txnRef ?? "").Split('-', 2);
            if (parts.Length == 2 && int.TryParse(parts[0], out var oid) && long.TryParse(parts[1], out var ts))
                return (oid, ts);
            return (0, 0);
        }

        private static string ToGmt7String(DateTime utc, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var gmt7 = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            return gmt7.ToString("yyyyMMddHHmmss");
        }

        private static string SanitizeOrderInfo(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Thanh toan don hang";
            var norm = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(input.Length);
            foreach (var ch in norm)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            // chỉ giữ a-z A-Z 0-9 và khoảng trắng
            var filtered = new StringBuilder(noDiacritics.Length);
            foreach (var ch in noDiacritics)
            {
                if (char.IsLetterOrDigit(ch) || ch == ' ')
                    filtered.Append(ch);
            }

            var result = filtered.ToString().Trim();
            if (result.Length == 0) result = "Thanh toan don hang";
            if (result.Length > 255) result = result.Substring(0, 255);
            return result;
        }
    }
}
