namespace MethaWebsite.Data
{
    public class Notifications
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public bool Account { get; set; } = true;
        public bool ShippingAndDelivery { get; set; } = true;
        public bool Deals { get; set; } = false;
        public bool SalesEvents { get; set; } = false;
        public bool SeasonalAndCurrentTrends { get; set; } = false;
        public bool ProductRecommendations { get; set; } = false;
        public bool NewReleases { get; set; } = false;
    }
}
