using MethaWebsite.Data;
using MethaWebsite.Data.ResponseModel;
using Microsoft.AspNetCore.SignalR;

namespace MethaWebsite.Services
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IConversationStore _store;

        public ChatHub(IChatService chatService, IConversationStore store)
        {
            _chatService = chatService;
            _store = store;
        }

        public async Task SendMessage(string userMessage)
        {
            var ctx = _store.GetOrCreate(Context.ConnectionId);
            var botResponse = await _chatService.GetBotResponseAsync(userMessage, ctx);
            await Clients.Caller.SendAsync("ReceiveMessage", "user", userMessage);
            await Clients.Caller.SendAsync("ReceiveMessage", "bot", botResponse);
        }

    }
}
