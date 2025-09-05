using MethaWebsite.Data;

namespace MethaWebsite.Services
{
    public interface IIntentRecognizer
    {
        IntentPrediction RecognizeIntent(string userMessage);
    }
}
