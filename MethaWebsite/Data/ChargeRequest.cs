namespace MethaWebsite.Data
{
    public class ChargeRequest
    {
        public string CustomerId { get; set; }
        public string PaymentMethodId { get; set; }
        public decimal Amount { get; set; }

    }
}
