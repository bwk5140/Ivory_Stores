namespace MethaWebsite.Data
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public List<string>? ProductIds { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public double ShippingCosts { get; set; }
        public string? ShippingAdressId { get; set; }
        public string? PaymentMethodId { get; set; }
        public bool HasShipped { get; set; } = false;
        public bool Cancelled { get; set; } = false;
        public int ShippingTime { get; set; }
    }
}
