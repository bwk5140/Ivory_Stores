namespace MethaWebsite.Data
{
    public class ShoppingCart
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public double Subtotal { get; set; }
        public bool IsSelected { get; set; }
        public string? UserId { get; set; }
        public List<string>? ProductIds { get; set; }
    }
}
