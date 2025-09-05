using MethaWebsite.Services;
using Mono.TextTemplating;

namespace MethaWebsite.Data.ResponseModel
{
    public class ConfirmContactDetailsHelpSlotFiller : ISlotFiller
    {
        private readonly ApplicationUserService _applicationUserService;
        private readonly IConversationStateStore _conversationStore;

        public ConfirmContactDetailsHelpSlotFiller(ApplicationUserService applicationUserService,
                                    IConversationStateStore conversationStore)
        {
            _applicationUserService = applicationUserService;
            _conversationStore = conversationStore;
        }
        Dictionary<string, string> ISlotFiller.FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            extractedSlots.TryGetValue("Email", out var email_response);
            extractedSlots.TryGetValue("Phone", out var phone_response);
            var state = _conversationStore.GetState(conversationId);
            if(state.PendingConfirmations.Any()){state.PendingConfirmations.Dequeue();}
            _conversationStore.SaveState(conversationId, state);

            if (email_response != null)
            {
                var user = _applicationUserService.GetApplicationUser();
                var email = user.Result.Email;
                return new Dictionary<string, string>
                {
                    ["Email"] = email,
                    ["Link"] = $"<a href='/Account/2FAVerification?ReturnUrl={"/Account/Manage"}' style='text-decoration: none;' target='_blank'>view your contact details</a>"
                };
            }
            if (phone_response != null)
            {
                var user = _applicationUserService.GetApplicationUser();
                var phone = user.Result.PhoneNumber;
                return new Dictionary<string, string>
                {
                    ["Phone"] = phone,
                    ["Link"] = $"<a href='/Account/2FAVerification?ReturnUrl={"/Account/Manage"}' style='text-decoration: none;' target='_blank'>view your contact details</a>"
                };
            }
            return new Dictionary<string, string>
            {
                ["Link"] = $"<a href='/Account/2FAVerification?ReturnUrl={"/Account/Manage"}' style='text-decoration: none;' target='_blank'>view your contact details</a>"
            };
        }
    }
}