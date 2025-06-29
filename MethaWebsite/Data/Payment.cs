namespace MethaWebsite.Data
{
    public class Payment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public string? BillingAddress { get; set; }
        public string? Type { get; set; }
        public string? LogoSource { get; set; }
        public string? ImageSource { get; set; }
    }
}
