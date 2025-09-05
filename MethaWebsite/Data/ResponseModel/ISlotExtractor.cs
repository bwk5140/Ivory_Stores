namespace MethaWebsite.Data.ResponseModel
{
    public interface ISlotExtractor
    {
        // Returns any slots it can infer; should respect provided SlotDefinitions
        IReadOnlyDictionary<string, SlotValue> Extract(ResponseRequest request, AnchorDefinition anchor, ConversationState state);
    }

}
