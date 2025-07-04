namespace MethaWebsite.Data
{
    public class ProductList
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public bool Private { get; set; }
        public string? UserId { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public List<string>? ProductIds { get; set; }
    }
}
