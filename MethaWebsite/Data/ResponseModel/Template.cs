using System.Xml.Xsl;

namespace MethaWebsite.Data.ResponseModel
{
    public sealed class Template
    {
        public required string TemplateId { get; init; }
        public required string AnchorId { get; init; }
        public required string Locale { get; init; }
        public required string Text { get; init; } // e.g., "It's {time} in {city}."
        public IReadOnlyList<string>? Conditions { get; init; } // tags, e.g., "has:city"
        public string? Tone { get; init; } // "neutral", "friendly"
        public Func<SlotValue, Task>? OnComplete { get; init; }

    }

}
