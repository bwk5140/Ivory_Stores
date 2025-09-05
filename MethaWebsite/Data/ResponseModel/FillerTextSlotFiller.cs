using MethaWebsite.Services;
using System.Net;

namespace MethaWebsite.Data.ResponseModel
{
    public class FillerTextSlotFiller : ISlotFiller
    {
        private readonly ApplicationUserService _userService;

        public FillerTextSlotFiller(ApplicationUserService userService)
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
            if (Username is not null)
            {
                extractedSlots.TryGetValue("FillerResponse", out var filler_text);
                if (filler_text != null)
                {

                    return new Dictionary<string, string>
                    {
                        ["FillerResponse"] = filler_text.Value,
                        ["UserName"] = user.Result == null ? "" : Username
                    };
                }
                extractedSlots.TryGetValue("Yes", out var yesResponse);
                if (yesResponse is not null)
                {
                    return new Dictionary<string, string>
                    {
                        ["Yes"] = yesResponse.Value,
                        ["UserName"] = user.Result == null ? "" : Username
                    };
                }
                extractedSlots.TryGetValue("No", out var noResponse);
                if (noResponse is not null)
                {
                    return new Dictionary<string, string>
                    {
                        ["No"] = noResponse.Value,
                        ["UserName"] = user.Result == null ? "" : Username
                    };
                }
                extractedSlots.TryGetValue("Gratitude", out var gratitude_text);
                if (gratitude_text != null)
                {
                    return new Dictionary<string, string>
                    {
                        ["Gratitude"] = gratitude_text.Value,
                        ["UserName"] = user.Result == null ? "" : Username
                    };
                }
                return new Dictionary<string, string>
                {
                    ["UserName"] = user.Result == null ? "" : Username
                };
            }
            
            return new Dictionary<string, string>();
        }
    }
}