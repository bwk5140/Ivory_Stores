using MethaWebsite.Services;

namespace MethaWebsite.Data.ResponseModel
{
    public class BlockedAccountHelpSlotFiller : ISlotFiller
    {
        private readonly ApplicationUserService _userService;
        public BlockedAccountHelpSlotFiller(ApplicationUserService userService)
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
                    ["UserName"] = user.Result == null ? "" : Username,
                    ["Link"] = $"<a href='/Account/2FAVerification?ReturnUrl={"/Account/Manage/ChangePassword"}' style='text-decoration: none;' target='_blank'>change your password</a>"
                };
            }
            return new Dictionary<string, string>
            {
                ["Link"] = $"<a href='/Account/2FAVerification?ReturnUrl={"/Account/Manage/ChangePassword"}' style='text-decoration: none;' target='_blank'>change your password</a>"
            };
        }
    }
}