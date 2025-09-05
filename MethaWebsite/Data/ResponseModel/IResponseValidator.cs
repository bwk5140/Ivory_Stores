namespace MethaWebsite.Data.ResponseModel
{
    public interface IResponseValidator
    {
        IReadOnlyList<string> Validate(
            ResponseRequest request,
            AnchorDefinition anchor,
            IReadOnlyDictionary<string, SlotValue> slots);
    }

}
