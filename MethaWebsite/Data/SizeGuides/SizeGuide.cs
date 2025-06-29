namespace MethaWebsite.Data.SizeGuides
{
    public class SizeGuide
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ProductId { get; set; }
        public DateTime? Created { get; set; } = DateTime.Now;
    }
}
