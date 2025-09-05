namespace MethaWebsite.Data.ResponseModel
{
    public record SlotValue
    {
        public required string Name { get; init; }
        public required string? Value { get; init; }
        public double Confidence { get; init; } = 0.0;
        public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
        public bool IsResolved { get; set; } = false; // post-resolution flag
    }

}
