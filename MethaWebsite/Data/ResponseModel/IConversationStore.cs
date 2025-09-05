namespace MethaWebsite.Data.ResponseModel
{
    public interface IConversationStore
    {
        ConversationContext GetOrCreate(string connectionId);
    }
}
