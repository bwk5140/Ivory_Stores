namespace MethaWebsite.Data.ResponseModel
{
    public sealed class ResponseRequest
    {
        public required string Utterance { get; init; }
        public required string IntentId { get; init; }
        public double IntentConfidence { get; init; }
        public string? Locale { get; init; } = "en-KE";
        public IDictionary<string, string>? Entities { get; init; }
    }

}
