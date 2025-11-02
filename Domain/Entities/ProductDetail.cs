using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema; 

namespace Domain.Entities
{
    [Table("ProductDetails")] 
    public partial class ProductDetail : BaseFullEntity
    {
        public int ProductId { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? Origin { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
