using Microsoft.ML.Data;

namespace MethaWebsite.Data
{
    public class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; }

        public float[] Score { get; set; }

    }
}
