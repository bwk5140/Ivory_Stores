namespace MethaWebsite.Data
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Type { get; set; }
        public string? Vendor { get; set; }
        public string? OrderId { get; set; }
        public string? UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
