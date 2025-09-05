using Microsoft.ML.Data;

namespace MethaWebsite.Data
{
    public class IntentPrediction
    {
        [ColumnName("PredictedLabel")] public string Intent;
        public float[] Score;
    }
}
