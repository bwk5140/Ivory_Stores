namespace MethaWebsite.Data
{
    public class Rating
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ProductReviewID { get; set; }
        public bool Selected { get; set; }
    }
}
