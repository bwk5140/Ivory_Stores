namespace MethaWebsite.Data.Interfaces
{
    public interface IVectorStore
    {
        Task<List<VectorSearchResult>> SearchAsync(ReadOnlyMemory<float> embedding, uint topK = 10);
    }
}
