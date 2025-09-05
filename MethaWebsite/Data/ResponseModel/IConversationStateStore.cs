namespace MethaWebsite.Data.ResponseModel
{
    public interface IConversationStateStore
    {
        ConversationState? GetState(string conversationId);
        void SaveState(string conversationId, ConversationState state);
    }
}
