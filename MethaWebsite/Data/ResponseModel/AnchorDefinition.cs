namespace MethaWebsite.Data.ResponseModel
{
    public sealed class AnchorDefinition
    {
        public required string AnchorId { get; init; } // e.g., "TimeQuery"
        public IReadOnlyList<SlotDefinition> Slots { get; init; } = new List<SlotDefinition>();
        public IReadOnlyList<string> TemplateIds { get; init; } = new List<string>();
        public double MinIntentConfidence { get; init; } = 0.6;
        public bool IsFallback { get; init; }
    }

}
