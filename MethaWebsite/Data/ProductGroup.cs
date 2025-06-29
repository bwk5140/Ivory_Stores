namespace MethaWebsite.Data
{
    public class ProductGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public int Stock { get; set; }
        public List<ProductColorGroup>? ProductColorGroups { get; set; }
    }
}
