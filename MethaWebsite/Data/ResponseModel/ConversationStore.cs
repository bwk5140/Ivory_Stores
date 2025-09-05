namespace MethaWebsite.Data.ResponseModel
{
    public class ConversationStore : IConversationStore
    {
        private ConversationContext _conversationContext;

        public ConversationStore(ConversationContext conversationContext)
        {
            _conversationContext = conversationContext;
        }
        public ConversationContext GetOrCreate(string connectionId)
        {
            if (_conversationContext == null)
            {
                _conversationContext = new ConversationContext(new ConversationState());
            }
            return _conversationContext;
        }
    }
}