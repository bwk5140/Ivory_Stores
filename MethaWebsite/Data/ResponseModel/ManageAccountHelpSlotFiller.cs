namespace MethaWebsite.Data.ResponseModel
{
    public class ManageAccountHelpSlotFiller : ISlotFiller
    {
        public Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId)
        {
            return new Dictionary<string, string>
            {
                ["Link"] = $"<a href='/Account/2FAVerification?ReturnUrl={"/Account/Manage"}' style='text-decoration: none;' target='_blank'>manage your account</a>"
            };
        }
    }
}