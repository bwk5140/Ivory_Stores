using Google.Protobuf.WellKnownTypes;

namespace MethaWebsite.Data.ResponseModel
{
    public sealed class ResponseResult
    {
        public required string Text { get; init; }
        public bool NeedsClarification { get; init; }
        public IReadOnlyDictionary<string, SlotValue> Slots { get; init; } = new Dictionary<string, SlotValue>();
        public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();
        public string? TemplateId { get; init; }
        public string? AnchorId { get; init; }
    }

}
