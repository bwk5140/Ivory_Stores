using MethaWebsite.Data;
using Microsoft.ML;

namespace MethaWebsite.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly string _modelPath = "sentiment_model.zip";
        private readonly MLContext _mlContext = new();
        ITransformer intentModel;
        private readonly List<SentimentDataLabeled> _trainingData = new();


        public TrainingService()
        {
            if (!File.Exists(_modelPath))
            {
                InitializeModel();
            }
        }
        public Task AddTrainingExampleAsync(string text, bool isPositive)
        {
            _trainingData.Add(new SentimentDataLabeled { Text = text, Label = isPositive });
            return Task.CompletedTask;
        }
        public static ITransformer TrainIntentModel(MLContext mlContext, string dataPath)
        {
            var data = mlContext.Data.LoadFromTextFile<IntentInput>(dataPath, hasHeader: true, separatorChar: ',');
            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(IntentInput.Text))
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(IntentInput.Label)))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            return pipeline.Fit(data);
        }
        public static ITransformer TrainSentimentModel(MLContext mlContext, string dataPath)
        {
            var data = mlContext.Data.LoadFromTextFile<SentimentInput>(dataPath, hasHeader: true, separatorChar: ',');

            var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            // Build pipeline
            var pipeline = mlContext.Transforms.Conversion
                .MapValueToKey("Label", "Label")
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", "Text"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // Train model
            return pipeline.Fit(split.TrainSet);
        }

        public static void TrainAndSaveModels()
        {
            var mlContext = new MLContext();

            var intentModel = TrainIntentModel(mlContext, "Models/amazon_intent_train.csv");
            mlContext.Model.Save(intentModel, null, "Models/intent_model.zip");
        }

        public Task RetrainModelAsync()
        {
            var mlContext = new MLContext();

            var dataView = mlContext.Data.LoadFromEnumerable(_trainingData);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);
            mlContext.Model.Save(model, dataView.Schema, _modelPath);

            return Task.CompletedTask;
        }

        private void InitializeModel()
        {
            var seedData = new List<SentimentDataLabeled>
        {
            new() { Text = "I love this!", Label = true },
            new() { Text = "This is terrible.", Label = false },
            new() { Text = "I'm very happy.", Label = true },
            new() { Text = "I hate this.", Label = false },
            new() { Text = "It's okay, not great.", Label = false }
        };

            var dataView = _mlContext.Data.LoadFromEnumerable(seedData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);
            _mlContext.Model.Save(model, dataView.Schema, _modelPath);
        }

        public class SentimentDataLabeled
        {
            public string Text { get; set; } = string.Empty;
            public bool Label { get; set; }
        }

    }
}
