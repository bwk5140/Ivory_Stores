namespace MethaWebsite.Data.ResponseModel
{
    public interface ISlotResolver
    {
        // Post-process to fill missing slots or normalize values (e.g., city → canonical)
        IReadOnlyDictionary<string, SlotValue> Resolve(
            ResponseRequest request,
            AnchorDefinition anchor,
            IReadOnlyDictionary<string, SlotValue> extracted);
    }

}
