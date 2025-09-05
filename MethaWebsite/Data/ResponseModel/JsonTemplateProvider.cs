namespace MethaWebsite.Data.ResponseModel
{
    public sealed class JsonTemplateProvider : ITemplateProvider
    {
        private readonly IReadOnlyDictionary<(string Anchor, string Locale), List<Template>> _byAnchorLocale;

        public JsonTemplateProvider(IEnumerable<Template> templates)
        {
            _byAnchorLocale = templates
                .GroupBy(t => (t.AnchorId.ToLowerInvariant(), t.Locale ?? "en-KE"))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public IReadOnlyList<Template> GetTemplates(string anchorId, string? locale)
        {
            var key = (Anchor: anchorId, Locale: locale ?? "en-KE");
            if (_byAnchorLocale.TryGetValue(key, out var list)) return list;
            // fallback to en-KE
            if (_byAnchorLocale.TryGetValue((anchorId, "en-KE"), out var fallback)) return fallback;
            return Array.Empty<Template>();
        }
    }
}
