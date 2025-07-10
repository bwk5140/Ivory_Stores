namespace MethaWebsite.Data
{
    public class ProductReview
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? Subject { get; set; }
        public DateTime? Created { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public string? ProductId { get; set; }
        public string? UserId { get; set; }
        public List<string>? ImageIds { get; set; }
        public List<Rating>? Ratings { get; set; }
    }
}
