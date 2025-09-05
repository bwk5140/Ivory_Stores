namespace MethaWebsite.Data.ResponseModel
{
    public class RespondRequest
    {
        public ConversationAction Action { get; set; } = default!;
        public ResponseRequest Request { get; set; } = default!;
    }
}
