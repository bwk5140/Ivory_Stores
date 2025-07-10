using MethaWebsite.Data;
using Pinecone;
using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class LocalEmbeddingService
    {
        private readonly HttpClient _http;
        public EmbeddingResponse embedding;

        public LocalEmbeddingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<EmbeddingResponse> GetEmbeddingAsync(string text)
        {
            var payload = new { text };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("http://localhost:5000/get-vector", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            embedding = JsonSerializer.Deserialize<EmbeddingResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return embedding;
        }
    }
}
