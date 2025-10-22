using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AccountDto
    {
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }
    }
}
