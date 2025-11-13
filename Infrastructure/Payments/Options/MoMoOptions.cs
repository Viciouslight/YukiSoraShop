namespace Infrastructure.Payments.Options
{
    public sealed class MoMoOptions
    {
        public string PartnerCode { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string PaymentUrl { get; set; } = default!;
        public string QueryUrl { get; set; } = default!;
        public bool IsProduction { get; set; }

        // Optional helpers
        public bool TestModeScaleDown { get; set; }
        public string? TestModeScaleDownMethod { get; set; }
        public long? TestModeScaleDownAmount { get; set; }

        // These can default to existing callback endpoints if you prefer
        public string? ReturnUrl { get; set; } // user returns here after payment
        public string? IpnUrl { get; set; }    // server-to-server notify
    }
}
