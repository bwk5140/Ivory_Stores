namespace MethaWebsite.Data
{
    public class ProductIssue
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Details { get; set; }
        public string? Comments { get; set; }
    }
}
