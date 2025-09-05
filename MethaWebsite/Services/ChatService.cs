using MethaWebsite.Data;
using MethaWebsite.Data.ResponseModel;
using Microsoft.AspNetCore.Components;
using Microsoft.ML;

namespace MethaWebsite.Services
{
    public class ChatService : IChatService
    {
        private PredictionEngine<SentimentInput, SentimentPrediction>? _engine;
        private readonly string _modelPath = "Models/sentiment_model.zip";
        SimpleIntentRecognizer recognizer = new SimpleIntentRecognizer();
        TemplateResponseProvider responder;
        private ChatBotService ChatBotService;

        public ChatService(WorldClockService worldClockService, HttpClient httpClient)
        {
            LoadModel();
            responder = new TemplateResponseProvider(worldClockService, httpClient);
            ChatBotService = new ChatBotService(recognizer, responder);
        }

        private void LoadModel()
        {
            var mlContext = new MLContext();
            var model = mlContext.Model.Load(_modelPath, out _);
            _engine = mlContext.Model.CreatePredictionEngine<SentimentInput, SentimentPrediction>(model);
        }

        public async Task<string> GetBotResponseAsync(string userInput , ConversationContext Context)
        {
            var prediction = _engine.Predict(new SentimentInput { Text = userInput, Label = "0" });
            var sentiment = prediction.PredictedLabel;
            var reply = await ChatBotService.GetBotReply(userInput);
            return reply;
            //Task.FromResult($"Your message seems {sentiment} (confidence: {prediction.Probability:P0}).");
        }

        public void ReloadModel() => LoadModel(); // Call after retraining

    }
}
