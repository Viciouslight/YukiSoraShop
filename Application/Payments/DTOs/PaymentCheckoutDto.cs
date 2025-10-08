using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.DTOs
{
    public sealed class PaymentCheckoutDto
    {
        public string CheckoutUrl { get; set; } = default!;
        public string Provider { get; set; } = "VNPay";
    }
}
