using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Account : BaseFullEntity
{
    public string UserName { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public AccountRole Role { get; set; } = AccountRole.Customer;

    public string? Status { get; set; }

    public bool IsExternal { get; set; } = false;

    public string? ExternalProvider { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
