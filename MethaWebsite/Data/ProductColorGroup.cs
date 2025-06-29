namespace MethaWebsite.Data
{
    public class ProductColorGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? ProductGroupID { get; set; }
        public List<Product>? Products { get; set; }
    }
}
