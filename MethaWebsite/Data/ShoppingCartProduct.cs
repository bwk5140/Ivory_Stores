namespace MethaWebsite.Data
{
    public class ShoppingCartProduct
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
