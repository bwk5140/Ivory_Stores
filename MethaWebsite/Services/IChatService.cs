using MethaWebsite.Data;

namespace MethaWebsite.Services
{
    public interface IChatService
    {
        Task<string> GetBotResponseAsync(string userInput, ConversationContext Context);
    }
}
