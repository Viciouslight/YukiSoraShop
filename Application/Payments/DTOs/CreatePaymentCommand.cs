using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.DTOs
{
    public sealed class CreatePaymentCommand
    {
        public int OrderId { get; set; }
        public string? BankCode { get; set; } // optional
        public string ClientIp { get; set; } = default!;
        public string? OrderDescription { get; set; }
        public string OrderTypeCode { get; set; } = "other";
    }
}
