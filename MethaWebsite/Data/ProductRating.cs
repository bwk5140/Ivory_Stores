namespace MethaWebsite.Data
{
    public class ProductRating
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? Subject { get; set; }
        public int Rating { get; set; }
        public DateTime? Created { get; set; }
        public string? Description { get; set; }
        public string? ProductId { get; set; }
        public string? UserId { get; set; }
        public List<ProductImage>? Images { get; set; }
    }
}
