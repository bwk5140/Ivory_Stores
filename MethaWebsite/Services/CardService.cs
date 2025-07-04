using Stripe;

namespace MethaWebsite.Services
{
    public class CardService
    {
        private readonly PaymentMethodService _paymentMethodService;

        public CardService()
        {
            _paymentMethodService = new PaymentMethodService();
        }

        public async Task<string> GetFingerprintAsync(string paymentMethodId)
        {
            var method = await _paymentMethodService.GetAsync(paymentMethodId);
            return method.Card.Fingerprint;
        }

    }
}
