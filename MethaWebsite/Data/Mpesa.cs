namespace MethaWebsite.Data
{
    public class Mpesa : Payment
    {
        public string? PhoneNumber { get; set; }
        public string? RegisteredName { get; set; }
        public bool DefaultPaymentMethod { get; set; }
    }
}
