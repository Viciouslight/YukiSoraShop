using Domain.Common;
using System.Collections.Generic;

namespace Application.Models;

public partial class Category : BaseFullEntity
{
    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

