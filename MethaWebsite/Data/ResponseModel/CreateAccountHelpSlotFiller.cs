using MethaWebsite.Services;

namespace MethaWebsite.Data.ResponseModel
{
    public class CreateAccountHelpSlotFiller : ISlotFiller
    {
        public Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            return new Dictionary<string, string>
            {
                ["Link"] = "<a href='/Account/Register' style='text-decoration: none;' target='_blank'>sign up</a>"
            };
        }
    }
}