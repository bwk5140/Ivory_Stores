
using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;

namespace MethaWebsite.Data.ResponseModel
{
    public class OrderHelpSlotFiller : ISlotFiller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IConversationStateStore _conversationStore;

        public OrderHelpSlotFiller(IDbContextFactory<ApplicationDbContext> dbFactory, 
                                    IConversationStateStore conversationStore)
        {
            _dbFactory = dbFactory;
            _conversationStore = conversationStore;
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
                    ["Link"] = $"<a href='/Account/Orders' style='text-decoration: none;' target='_blank'>view all orders</a>"
                };
            }
            extractedSlots.TryGetValue("OrderId", out var order_id);
            if (order_id != null)
            {
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
                using var context = _dbFactory.CreateDbContext();
                var order = context.Order.FirstOrDefault(o => o.Id == order_id.Value);
                if (order is not null)
                {
                    return new Dictionary<string, string>
                    {
                        ["OrderId"] = order_id.Value,
                        ["Link"] = $"<a href='/Account/Order/details?Id={order_id.Value}' style='text-decoration: none;' target='_blank'>view order details</a>"
                    };
                }
            }
            return new Dictionary<string, string>();
        }
    }
}