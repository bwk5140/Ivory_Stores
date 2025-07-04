namespace MethaWebsite.Data
{
    public class Shipping
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Type { get; set; }
        public double Cost { get; set; }
        public double BaseCost { get; set; }
        public double Distance { get; set; }
        public double Rate { get; set; }
    }
}
