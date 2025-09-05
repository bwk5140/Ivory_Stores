namespace MethaWebsite.Data.ResponseModel
{
    public class ConversationState
    {
        public string? AnchorId { get; set; }
        public Dictionary<string, SlotValue> FilledSlots { get; set; } = new();
        public List<string> PendingSlots { get; set; } = new();
        public string? LastPromptedSlot { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<ConversationTurn> History { get; set; } = new();
        public Queue<ConfirmationContext>? PendingConfirmations { get; set; } = new();
        public ConfirmationStage CurrentStage { get; set; } = ConfirmationStage.None;
    }
    public enum ConfirmationStage
    {
        None,
        ConfirmAddress,
        ConfirmCurrentAddress,
        ConfirmUpdateAddress,
        ConfirmAddNewAddress
    }
}
