namespace MethaWebsite.Data
{
    public class CreditDebitCard : Payment
    {
        public string? NameOnCard { get; set; }
        public int ExpirationMonth { get; set; } = new DateTime(DateTime.Now.Year, 1, 1).Month;
        public int ExpirationYear { get; set; } = new DateTime(DateTime.Now.Year, 1, 1).Year;
        public bool DefaultPaymentMethod { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? Fingerprint { get; set; }

    }
}
