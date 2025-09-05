using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data
{
    public class Product
    {
        [Required]
        public string? Name { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? Description { get; set; }
        [Required(ErrorMessage = "The Category field is required")]
        public string? CategoryId { get; set; }
        [Required(ErrorMessage = "Please add a size chart")]
        public string? SizeGuideId { get; set; }
        [Required]
        public string? Gender { get; set; }
        public double DiscountedPrice { get; set; }
        public int Discount { get; set; }
        public double DeliveryFee { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public string? ShippingId { get; set; }
        public string? ProductColorGroupID { get; set; }
        [Required]
        public string? Brand { get; set; }
        [Required]
        public int Stock { get; set; }
        public int Quantity { get; set; } = 1;
        public double Rating { get; set; }
        public float[]? Embedding { get; set; }
        public float Norm { get; set; }
        public double RatingTotal { get; set; }
        [Required]
        public string? Color { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string? Size { get; set; }
        [Required]
        public string? Fabric { get; set; }
        [Required]
        public string? Lifestyle { get; set; }
        [Required]
        public string? WashInstructions { get; set; }
        public List<string>? ProductIssueIds { get; set; }
        public List<ProductImage>? Images { get; set; }
        public List<string>? ProductReviewIds { get; set; }
    }
}
