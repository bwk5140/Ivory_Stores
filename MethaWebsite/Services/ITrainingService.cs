namespace MethaWebsite.Services
{
    public interface ITrainingService
    {
        public interface ITrainingService
        {
            Task AddTrainingExampleAsync(string text, bool isPositive);
            Task RetrainModelAsync();
        }

    }
}
