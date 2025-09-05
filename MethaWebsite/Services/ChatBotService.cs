namespace MethaWebsite.Services
{
    public class ChatBotService
    {
        private readonly IIntentRecognizer _recognizer;
        private readonly IResponseProvider _responder;

        public ChatBotService(IIntentRecognizer recognizer, IResponseProvider responder)
        {
            _recognizer = recognizer;
            _responder = responder;
        }

        public async Task<string> GetBotReply(string userMessage)
        {
            var intent = _recognizer.RecognizeIntent(userMessage);
            return await _responder.GetResponse(intent.Intent, userMessage);
        }
        public string ModulateReply(string reply, string sentiment, string intent)
        {
            return _responder.ModulateResponse(reply, sentiment, intent);
        }
    }

}
