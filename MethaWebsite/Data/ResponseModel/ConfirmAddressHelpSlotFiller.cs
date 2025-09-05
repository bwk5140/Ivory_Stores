using MethaWebsite.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MethaWebsite.Data.ResponseModel
{
    public class ConfirmAddressHelpSlotFiller : ISlotFiller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IConversationStateStore _conversationStore;

        public ConfirmAddressHelpSlotFiller(IDbContextFactory<ApplicationDbContext> dbFactory,
                                    IConversationStateStore conversationStore)
        {
            _dbFactory = dbFactory;
            _conversationStore = conversationStore;
        }
        Dictionary<string, string> ISlotFiller.FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            extractedSlots.TryGetValue("Address", out var address_response);
            if (address_response is not null)
            {
                using var context = _dbFactory.CreateDbContext();
                var address = context.Address.FirstOrDefault(a => a.AddressLine1.Contains(address_response.Value)
                                                                || a.AddressLine2.Contains(address_response.Value));
                if (address != null)
                {
                    return new Dictionary<string, string>
                    {
                        ["Address"] = address.FullName + ",\n" + address.AddressLine1 + ",\n" + address.AddressLine2 + ",\n" + address.City + ",\n" + address.Country,
                        ["Link"] = $"<a href='/Addresses' style='text-decoration: none;' target='_blank'>view your addresses</a>"
                    };
                }
            }
            return new Dictionary<string, string>
            {
                ["Link"] = $"<a href='/Addresses' style='text-decoration: none;' target='_blank'>view your addresses</a>"
            };
        }
    }
}