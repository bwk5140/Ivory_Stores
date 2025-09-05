namespace MethaWebsite.Data.ResponseModel
{
    public class ConversationTurn
    {
        public string? UserInput { get; set; }
        public Dictionary<string, SlotValue> SlotChanges { get; set; } = new();
        public string? PromptedSlot { get; set; }
        public DateTime Timestamp { get; set; }
    }
}