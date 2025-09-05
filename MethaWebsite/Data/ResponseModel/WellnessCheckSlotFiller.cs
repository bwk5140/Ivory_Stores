using MethaWebsite.Services;

namespace MethaWebsite.Data.ResponseModel
{
    public class WellnessCheckSlotFiller : ISlotFiller
    {
        private readonly ApplicationUserService _userService;

        public WellnessCheckSlotFiller(ApplicationUserService userService)
        {
            _userService = userService;
        }
        public Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string converationId)
        {
            extractedSlots.TryGetValue("WellnessCheckType", out var wellness_check);
            var user = _userService.GetApplicationUser();
            if (wellness_check is not null)
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
                        ["WellnessCheckType"] = wellness_check.Value,
                        ["UserName"] = user.Result == null ? "" : Username
                    };
                }
            }
            return new Dictionary<string, string>();
        }
    }
}