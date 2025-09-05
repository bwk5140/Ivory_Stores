namespace MethaWebsite.Data.ResponseModel
{
    public sealed class SlotDefinition
    {
        public required string Name { get; init; }
        public bool Required { get; init; } = false;
        public string? Type { get; init; } // e.g., "City", "Date", "Action"
        public string? Regex { get; init; } // pattern for regex extraction
        public string? OnFill { get; set; }            // Name of the primary action handler
        public string? Fallback { get; set; }          // Optional fallback handler
        public bool IsGlobal { get; init; } = false;

    }

}
