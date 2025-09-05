using MethaWebsite.Data.ResponseModel;

namespace MethaWebsite.Data
{
    public class ConversationContext
    {
        private readonly int _maxTurns = 5;
        private readonly Queue<ChatMessage> _history = new();
        private ConversationState _conversationState;

        public ConversationContext(ConversationState conversationState)
        {
            _conversationState = conversationState;
        }
        public void AddMessage(string role, string text, string intent)
        {
            if (_history.Count >= _maxTurns)
                _history.Dequeue();
            var chatMessage = new ChatMessage(role, text, intent, "default", DateTime.UtcNow);
            _history.Enqueue(chatMessage);
        }
        public ConversationState GetConversationState() { return _conversationState; }
        public string GetContextText() =>
            string.Join(" ", _history.Select(m => m.Text));

        public string GetLastBotMessage() =>
            _history.LastOrDefault(m => m.Role=="bot")?.Text ?? "";
        public string GetLastUserMessage() =>
            _history.LastOrDefault(m => m.Role=="user")?.Text ?? "";
    }

}
