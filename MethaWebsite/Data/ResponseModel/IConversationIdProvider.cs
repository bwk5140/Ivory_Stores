namespace MethaWebsite.Data.ResponseModel
{
    public interface IConversationIdProvider
    {
        string GetConversationId(HttpContext context);

    }
}
