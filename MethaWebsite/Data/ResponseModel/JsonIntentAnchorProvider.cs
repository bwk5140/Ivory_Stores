namespace MethaWebsite.Data.ResponseModel
{
    public sealed class JsonIntentAnchorProvider : IIntentAnchorProvider
    {
        private readonly IReadOnlyDictionary<string, string> _intentToAnchor;
        private readonly IReadOnlyDictionary<string, AnchorDefinition> _anchors;
        private readonly string? _fallbackAnchorId;

        public JsonIntentAnchorProvider(
            IReadOnlyDictionary<string, string> intentToAnchor,
            IReadOnlyDictionary<string, AnchorDefinition> anchors)
        {
            _intentToAnchor = intentToAnchor;
            _anchors = anchors;
            _fallbackAnchorId = anchors.Values.FirstOrDefault(a => a.IsFallback)?.AnchorId;
        }

        public string? GetAnchorForIntent(string intentId) =>
            _intentToAnchor.TryGetValue(intentId.Trim().ToLowerInvariant(), out var anchor) ? anchor : null;

        public AnchorDefinition? GetAnchorDefinition(string anchorId) =>
            _anchors.TryGetValue(anchorId, out var def) ? def : GetFallbackAnchor();

        public AnchorDefinition GetFallbackAnchor() => _fallbackAnchorId != null ? GetAnchorDefinition(_fallbackAnchorId) : null;

        public IEnumerable<AnchorDefinition> GetAllAnchors()
        {
            return _anchors.Values;
        }
    }
}
