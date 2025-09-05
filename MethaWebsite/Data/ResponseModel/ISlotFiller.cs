namespace MethaWebsite.Data.ResponseModel
{
    public interface ISlotFiller
    {
        Dictionary<string, string> FillSlots(IReadOnlyDictionary<string, SlotValue> extractedSlots, string conversationId);
    }
}
