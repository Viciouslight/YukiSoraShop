using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Infrastructure.Payments.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments.Providers.MoMo
{
    public sealed class MoMoPaymentGateway : IMoMoGateway
    {
        private readonly MoMoOptions _opt;
        private readonly ILogger<MoMoPaymentGateway> _logger;
        private readonly IHttpClientFactory _httpFactory;

        public MoMoPaymentGateway(IOptions<MoMoOptions> opt, ILogger<MoMoPaymentGateway> logger, IHttpClientFactory httpFactory)
        {
            _opt = opt.Value;
            _logger = logger;
            _httpFactory = httpFactory;
        }

        public async Task<PaymentCheckoutDTO> GenerateCheckoutUrlAsync(
            int orderId,
            decimal amountVnd,
            string clientIp,
            string? orderDesc,
            CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var momoOrderId = $"{orderId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var requestId = Guid.NewGuid().ToString("N");
            var requestType = "captureWallet";
            var orderInfo = string.IsNullOrWhiteSpace(orderDesc) ? $"Thanh toan don hang #{orderId}" : orderDesc;
            var amount = ((long)decimal.Round(amountVnd, 0, MidpointRounding.AwayFromZero)).ToString();

            var redirectUrl = _opt.ReturnUrl ?? string.Empty;
            var ipnUrl = _opt.IpnUrl ?? string.Empty;
            var extraData = string.Empty;

            var rawSignature =
                $"accessKey={_opt.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={momoOrderId}&orderInfo={orderInfo}&partnerCode={_opt.PartnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSha256Hex(_opt.SecretKey, rawSignature);

            var body = $$"""
            {
              "partnerCode": "{{_opt.PartnerCode}}",
              "accessKey": "{{_opt.AccessKey}}",
              "requestId": "{{requestId}}",
              "amount": "{{amount}}",
              "orderId": "{{momoOrderId}}",
              "orderInfo": "{{EscapeForJson(orderInfo)}}",
              "redirectUrl": "{{redirectUrl}}",
              "ipnUrl": "{{ipnUrl}}",
              "lang": "vi",
              "requestType": "{{requestType}}",
              "extraData": "{{extraData}}",
              "signature": "{{signature}}"
            }
            """;

            var http = _httpFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, _opt.PaymentUrl);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var resp = await http.SendAsync(req, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("MoMo create payment failed: {Status} {Body}", (int)resp.StatusCode, json);
                throw new InvalidOperationException("MoMo gateway error");
            }

            // Very light-weight parse to find payUrl without adding JSON libs here
            var payUrl = TryExtractJsonValue(json, "payUrl") ?? TryExtractJsonValue(json, "deeplink");
            if (string.IsNullOrWhiteSpace(payUrl))
            {
                _logger.LogError("MoMo response missing payUrl: {Body}", json);
                throw new InvalidOperationException("MoMo gateway: invalid response");
            }

            _logger.LogInformation("Generated MoMo checkout URL for Order {OrderId}: {Url}", orderId, payUrl);
            return new PaymentCheckoutDTO
            {
                CheckoutUrl = payUrl,
                Provider = "MoMo"
            };
        }

        private static string HmacSha256Hex(string secret, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static string EscapeForJson(string s) =>
            (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string? TryExtractJsonValue(string json, string key)
        {
            // naive extract: "key":"value"
            var needle = $"\"{key}\":";
            var idx = json.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var start = json.IndexOf('"', idx + needle.Length);
            if (start < 0) return null;
            var end = json.IndexOf('"', start + 1);
            if (end < 0) return null;
            return json.Substring(start + 1, end - start - 1);
        }
    }
}
