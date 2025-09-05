namespace MethaWebsite.Data.ResponseModel
{
    public class ConversationAction
    {
        public ConversationActionType Type { get; set; }
        public string? SlotToPrompt { get; set; }
        public string? AnchorId { get; set; }
        public IReadOnlyDictionary<string, SlotValue>? FilledSlots { get; set; }
        public string? ClarificationText { get; set; }

        public static ConversationAction PromptForSlot(string slotName, string anchorId)
        {
            return new ConversationAction
            {
                Type = ConversationActionType.PromptForSlot,
                SlotToPrompt = slotName,
                AnchorId = anchorId
            };
        }
        public static ConversationAction PromptForSlot(string clarificationText, string slotName, string anchorId, IReadOnlyDictionary<string, SlotValue> slots)
        {
            return new ConversationAction
            {
                Type = ConversationActionType.PromptForSlot,
                SlotToPrompt = slotName,
                AnchorId = anchorId,
                ClarificationText = clarificationText,
                FilledSlots = slots
            };
        }

        public static ConversationAction ExecuteAnchor(string anchorId, IReadOnlyDictionary<string, SlotValue> slots)
        {
            if (slots.TryGetValue(anchorId, out var slotValue))
            {
                slotValue.IsResolved = true;
            }
            return new ConversationAction
            {
                Type = ConversationActionType.ExecuteAnchor,
                AnchorId = anchorId,
                FilledSlots = slots
            };
        }

        public static ConversationAction Clarify(string text, string anchorId, IReadOnlyDictionary<string, SlotValue> slots)
        {
            return new ConversationAction
            {
                Type = ConversationActionType.Clarify,
                ClarificationText = text,
                AnchorId = anchorId,
                FilledSlots = slots
            };
        }

        public static ConversationAction Fallback(string text)
        {
            return new ConversationAction
            {
                Type = ConversationActionType.Fallback,
                ClarificationText = text
            };
        }
    }
    public enum ConversationActionType
    {
        PromptForSlot,
        ExecuteAnchor,
        Clarify,
        Fallback,
        End
    }
}