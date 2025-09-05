namespace MethaWebsite.Data.ResponseModel
{
    public interface ITemplateRenderer
    {
        string Render(string templateText, IReadOnlyDictionary<string, SlotValue> slots);
    }

}
