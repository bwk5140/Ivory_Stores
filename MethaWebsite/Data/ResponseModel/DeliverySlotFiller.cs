using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;

namespace MethaWebsite.Data.ResponseModel
{
    public class DeliverySlotFiller : ISlotFiller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IConversationStateStore _conversationStore;
        private readonly ShippingCalculator _shippingCalculator;

        public DeliverySlotFiller(IDbContextFactory<ApplicationDbContext> dbFactory,
                                    IConversationStateStore conversationStore,
                                    ShippingCalculator shippingCalculator)
        {
            _dbFactory = dbFactory;
            _conversationStore = conversationStore;
            _shippingCalculator = shippingCalculator;
        }
        Dictionary<string, string> ISlotFiller.FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            var state = _conversationStore.GetState(conversationId);
            state.LastPromptedSlot = null;
            if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
            _conversationStore.SaveState(conversationId, state);
            extractedSlots.TryGetValue("UserResponse", out var user_response);
            if (user_response != null)
            {
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                return new Dictionary<string, string>
                {
                    ["UserResponse"] = user_response.Value,
                    ["Link"] = $"<a href='/Account/Orders' style='text-decoration: none;' target='_blank'>track all orders</a>"
                };
            }
            extractedSlots.TryGetValue("Yes", out var yes_response);
            if (yes_response != null)
            {
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                return new Dictionary<string, string>();
            }
            extractedSlots.TryGetValue("No", out var no_response);
            if (no_response != null)
            {
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                return new Dictionary<string, string>();
            }
            extractedSlots.TryGetValue("Address", out var address_response);
            if (address_response != null)
            {
                var ShippingDistance = _shippingCalculator.GetDistance(address_response.Value);
                var StandardShippingCost = ShippingDistance.Result * 3.5 + 150;
                var FastShippingCost = ShippingDistance.Result * 3.5 + 300;
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                return new Dictionary<string, string>
                {
                    ["Address"] = address_response.Value,
                    ["StandardShippingCost"] = StandardShippingCost.ToString(),
                    ["FastShippingCost"] = FastShippingCost.ToString()
                };
            }
            extractedSlots.TryGetValue("OrderId", out var order_id);
            if (order_id != null)
            {
                state.LastPromptedSlot = null;
                if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                using var context = _dbFactory.CreateDbContext();
                var order = context.Order.FirstOrDefault(o => o.Id == order_id.Value);
                if (order is not null)
                {
                    return new Dictionary<string, string>
                    {
                        ["OrderId"] = order_id.Value,
                        ["Link"] = $"<a href='/Account/Order/details/track?Id={order_id.Value}' style='text-decoration: none;' target='_blank'>track your order</a>"
                    };
                }
            }
            return new Dictionary<string, string>();
        }
    }
}