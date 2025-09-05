using MethaWebsite.Data;
using Microsoft.ML;

namespace MethaWebsite.Services
{
    public class SimpleIntentRecognizer : IIntentRecognizer
    {
        private readonly PredictionEngine<IntentInput, IntentPrediction> _engine;

        public SimpleIntentRecognizer()
        {
            var mlContext = new MLContext();
            var model = mlContext.Model.Load("Models/intent_model.zip", out _);
            _engine = mlContext.Model.CreatePredictionEngine<IntentInput, IntentPrediction>(model);
        }

        public IntentPrediction RecognizeIntent(string message)
        {
            var input = new IntentInput { Text = message.ToLowerInvariant() };
            var prediction = _engine.Predict(input);
            return prediction;
        }
    }

}
