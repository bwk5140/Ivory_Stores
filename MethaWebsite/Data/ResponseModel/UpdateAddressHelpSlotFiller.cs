using MethaWebsite.Data.Contexts;
using MethaWebsite.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MethaWebsite.Data.ResponseModel
{
    public class UpdateAddressHelpSlotFiller : ISlotFiller
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IConversationStateStore _conversationStore;
        private readonly SlotActionBinder _slotActionBinder;
        private readonly ApplicationUserService _userService;

        public UpdateAddressHelpSlotFiller(IDbContextFactory<ApplicationDbContext> dbFactory,
                                    IConversationStateStore conversationStore, SlotActionBinder slotActionBinder,
                                    ApplicationUserService userService)
        {
            _dbFactory = dbFactory;
            _conversationStore = conversationStore;
            _slotActionBinder = slotActionBinder;
            _userService = userService;
        }
        Dictionary<string, string> ISlotFiller.FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            var state = _conversationStore.GetState(conversationId);
            //state.LastPromptedSlot = null;
            //if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
            //_conversationStore.SaveState(conversationId, state);
            var user = _userService.GetApplicationUser().Result;

            extractedSlots.TryGetValue("Address", out var address_response);
            extractedSlots.TryGetValue("NewAddress", out var new_address_response);
            extractedSlots.TryGetValue("address_confirmation_yes", out var addressYesResponse);
            extractedSlots.TryGetValue("add_address_confirmation_yes", out var addAddressYesResponse);
            extractedSlots.TryGetValue("address_confirmation_no", out var addressNoResponse);
            extractedSlots.TryGetValue("update_address_confirmation_yes", out var updateAddressYesResponse);


            if (address_response is not null)
            {
                using var context = _dbFactory.CreateDbContext();
                var address = context.Address.FirstOrDefault(a => (a.AddressLine1.Contains(address_response.Value)
                                                                || a.AddressLine2.Contains(address_response.Value)) && a.UserId == user.Id);

                if (addressYesResponse is not null)
                {
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                    state.FilledSlots.Clear();
                    _conversationStore.SaveState(conversationId, state);
                }

                if (addressNoResponse is not null)
                {
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                    state.FilledSlots.Clear();
                    _conversationStore.SaveState(conversationId, state);
                }
                if (updateAddressYesResponse is not null)
                {
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                    state.FilledSlots.Clear();
                    _conversationStore.SaveState(conversationId, state);
                }
                if (new_address_response is not null)
                {
                    if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Clear(); }
                    state.FilledSlots.Clear();
                    _conversationStore.SaveState(conversationId, state);
                    _slotActionBinder.HandleSlotAsync(new_address_response);
                }
                if (address != null)
                {
                    return new Dictionary<string, string>
                    {
                        ["Address"] = address.FullName + ",\n" + address.AddressLine1 + ",\n" + address.AddressLine2 + ",\n" + address.City + ",\n" + address.Country,
                        ["Link"] = $"<a href='/Addresses' style='text-decoration: none;' target='_blank'>update your addresses</a>"
                    };
                }
                else
                {
                    return new Dictionary<string, string>
                    {
                        ["Address"] = address_response.Value,
                        ["Link"] = $"<a href='/Addresses' style='text-decoration: none;' target='_blank'>update your addresses</a>"
                    };
                }
            }
            else
            {
                if (state.PendingConfirmations.Any()) { state.PendingConfirmations.Dequeue(); }
                state.FilledSlots.Clear();
                _conversationStore.SaveState(conversationId, state);
            }
            
            return new Dictionary<string, string>
            {
                ["Link"] = $"<a href='/Addresses' style='text-decoration: none;' target='_blank'>update your addresses</a>"
            };
        }
    }
}