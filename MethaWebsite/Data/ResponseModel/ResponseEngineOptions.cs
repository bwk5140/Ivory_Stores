using Microsoft.Extensions.Logging;

namespace MethaWebsite.Data.ResponseModel
{
    public sealed class ResponseEngineOptions
    {
        public double? GlobalMinIntentConfidence { get; set; }
        public bool LogSlotValues { get; set; } = false;
        public LogLevel MissingAnchorLogLevel { get; set; } = LogLevel.Warning;
        public string? LowConfidenceFallbackText { get; set; }
    }

}
