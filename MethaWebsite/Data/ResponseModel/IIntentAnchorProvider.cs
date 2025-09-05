namespace MethaWebsite.Data.ResponseModel
{
    public interface IIntentAnchorProvider
    {
        string? GetAnchorForIntent(string intentId);
        AnchorDefinition? GetAnchorDefinition(string anchorId);
        AnchorDefinition GetFallbackAnchor();
        IEnumerable<AnchorDefinition> GetAllAnchors();
    }

}
