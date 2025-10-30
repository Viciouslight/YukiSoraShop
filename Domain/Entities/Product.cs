using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace Domain.Entities
{
    [Table("Products")] 
    public partial class Product : BaseFullEntity
    {
        [Required]
        [StringLength(255)]
        public string ProductName { get; set; } = null!;
        [StringLength(2000)]
        public string? Description { get; set; }
        [Range(0, 999999999)]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        [Required]
        [StringLength(255)]
        public string CategoryName { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
