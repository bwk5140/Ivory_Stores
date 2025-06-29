namespace MethaWebsite.Data
{
    public class ProductList
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public bool Private { get; set; }
        public List<Product>? Products { get; set; }
    }
}
