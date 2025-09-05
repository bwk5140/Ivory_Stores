using Microsoft.ML.Data;

namespace MethaWebsite.Data
{
    public class IntentInput
    {
        [LoadColumn(0)] public string Label { get; set; }
        [LoadColumn(1)] public string Text { get; set; }

    }
}
