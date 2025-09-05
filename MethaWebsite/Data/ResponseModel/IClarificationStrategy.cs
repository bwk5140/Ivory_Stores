namespace MethaWebsite.Data.ResponseModel
{
    public interface IClarificationStrategy
    {
        string BuildClarification(ResponseRequest request, AnchorDefinition anchor, IReadOnlyList<string> issues);
    }

}
