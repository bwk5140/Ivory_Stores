using MethaWebsite.Data;
using Microsoft.ML;

namespace MethaWebsite.Services
{
    public class MlPredictionService
    {
        private readonly PredictionEngine<IntentInput, IntentPrediction> _intentEngine;
        private readonly PredictionEngine<SentimentInput, SentimentPrediction> _sentimentEngine;

        public MlPredictionService()
        {
            var mlContext = new MLContext();

            var intentModel = mlContext.Model.Load("Models/intent_model.zip", out var schema);
            var sentimentModel = mlContext.Model.Load("Models/sentiment_model.zip", out _);

            _intentEngine = mlContext.Model.CreatePredictionEngine<IntentInput, IntentPrediction>(intentModel);
            _sentimentEngine = mlContext.Model.CreatePredictionEngine<SentimentInput, SentimentPrediction>(sentimentModel);
        }

        public (string intent, string sentiment, float[] score) AnalyzeText(string text)
        {
            var intent = _intentEngine.Predict(new IntentInput { Text = text.ToLowerInvariant() });
            var sentiment = _sentimentEngine.Predict(new SentimentInput { Text = text, Label = "0" });

            return (intent.Intent, sentiment.PredictedLabel, intent.Score);
        }
    }
}
