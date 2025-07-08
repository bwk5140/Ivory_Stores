using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class OpenAIEmbeddingService
    {
        private readonly HttpClient _http;
        private const string Endpoint = "https://api.openai.com/v1/embeddings";
        private string ApiKey; // Replace with your key

        public OpenAIEmbeddingService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            ApiKey = configuration["OpenAI:ApiKey"];
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var payload = new
            {
                input = text,
                model = "text-embedding-3-small"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Endpoint, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var vector = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return vector;
        }
    }
}
