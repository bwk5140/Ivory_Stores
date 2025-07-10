namespace MethaWebsite.Services
{
    using Stripe;
    using System.Threading.Tasks;

    public class CardPaymentService
    {
        private readonly PaymentIntentService _intentService;
        public CardPaymentService(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
            _intentService = new PaymentIntentService();
        }

        public Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency = "KES")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            return _intentService.CreateAsync(options);
        }
    }
}
