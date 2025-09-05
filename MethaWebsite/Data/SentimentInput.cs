using Microsoft.ML.Data;

namespace MethaWebsite.Data
{
    public class SentimentInput
    {
        [LoadColumn(0)] public string Text { get; set; }
        [LoadColumn(1)] public string Label { get; set; }

    }
}
