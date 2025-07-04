using MethaWebsite.Data;
using Stripe;

namespace MethaWebsite.Services
{
    public class CardSetupSevice
    {
        public CardSetupSevice(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        }

        public async Task<SetupIntent> CreateSetupIntentAsync(string customerId)
        {
            var options = new SetupIntentCreateOptions
            {
                Customer = customerId
            };

            var service = new SetupIntentService();
            return await service.CreateAsync(options);
        }
        public async Task<CreditDebitCard> GetSavedCardDetailsAsync(string paymentMethodId)
        {
            var pmService = new PaymentMethodService();
            var paymentMethod = await pmService.GetAsync(paymentMethodId);

            var card = paymentMethod.Card;
            return new CreditDebitCard
            {
                Type = card.Brand,
                CardNumber = card.Last4,
                ExpirationMonth = (int)card.ExpMonth,
                ExpirationYear = (int)card.ExpYear,
                Fingerprint = card.Fingerprint
            };
        }
    }
}
