using MethaWebsite.Services;

namespace MethaWebsite.Data.ResponseModel
{
    public class TroubleWithAccountHelpSlotFiller : ISlotFiller
    {
        private readonly ApplicationUserService _userService;
        public TroubleWithAccountHelpSlotFiller(ApplicationUserService userService)
        {
            _userService = userService;
        }
        public Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            var user = _userService.GetApplicationUser();
            var index = user.Result.Name.IndexOf(" ");
            if (index == -1)
            {
                index = user.Result.Name.Length - 1;
            }
            var Username = user.Result.Name.Substring(0, index);
            if (Username != null)
            {
                return new Dictionary<string, string>
                {
                    ["UserName"] = user.Result == null ? "" : Username
                };
            }
            return new Dictionary<string, string>();
        }
    }
}