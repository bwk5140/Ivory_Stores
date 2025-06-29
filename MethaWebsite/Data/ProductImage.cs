namespace MethaWebsite.Data
{
    public class ProductImage
    {
        public byte[]? Image { get; set; }
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? Created { get; set; } = DateTime.Now;
        public string? ProductColorGroupId { get; set; }
    }
}
