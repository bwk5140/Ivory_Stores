using MethaWebsite.Data;
using MethaWebsite.Data.Interfaces;
using Pinecone;

public class PineconeVectorStore : IVectorStore
{
    private readonly IndexClient _index;

    public PineconeVectorStore(PineconeClient client, string indexName)
    {
        _index = client.Index(indexName);
    }

    public async Task<List<VectorSearchResult>> SearchAsync(ReadOnlyMemory<float> embedding, uint topK = 10)
    {
        var query = new Pinecone.QueryRequest
        {
            Vector = embedding.ToArray(),
            TopK = topK,
            IncludeMetadata = true
        };

        var result = await _index.QueryAsync(query);

        return result.Matches.Select(m => new VectorSearchResult
        {
            Id = m.Id,
            Score = m.Score
        }).ToList();
    }
}