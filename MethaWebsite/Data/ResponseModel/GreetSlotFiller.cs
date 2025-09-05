using MethaWebsite.Services;

namespace MethaWebsite.Data.ResponseModel
{
    public class GreetSlotFiller : ISlotFiller
    {
        private readonly ApplicationUserService _userService;
        private readonly IConversationStateStore _conversationStore;
        public GreetSlotFiller(ApplicationUserService userService, IConversationStateStore conversationStore)
        {
            _userService = userService;
            _conversationStore = conversationStore;
        }
        public Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string converationId)
        {
            //var state = _conversationStore.GetState(converationId);
            //state.AnchorId = null;
            //_conversationStore.SaveState(converationId, state);
            extractedSlots.TryGetValue("GreetingType", out var greet);
            var user = _userService.GetApplicationUser();
            if (greet is not null)
            {
                if (user is not null)
                {
                    var index = user.Result.Name.IndexOf(" ");
                    if (index == -1)
                    {
                        index = user.Result.Name.Length - 1;
                    }
                    var Username = user.Result.Name.Substring(0, index);
                    return new Dictionary<string, string>
                    {
                        ["GreetingType"] = greet.Value,
                        ["UserName"] = user.Result == null ? "" : Username
                    };
                }
            }
            return new Dictionary<string, string>();
        }
    }
}