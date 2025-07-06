using OpenAI;
using OpenAI.Embeddings;

public class EmbeddingService
{
    private readonly EmbeddingClient client;

    public EmbeddingService(IConfiguration config)
    {
        client = new EmbeddingClient("text-embedding-3-small", config["OpenAI:ApiKey"]);
    }

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input)
    {
        OpenAIEmbedding embedding = client.GenerateEmbedding(input);
        ReadOnlyMemory<float> vector = embedding.ToFloats();

        return vector;
    }
}