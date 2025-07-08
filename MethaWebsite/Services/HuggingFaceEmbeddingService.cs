using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class HuggingFaceEmbeddingService
    {
        private readonly HttpClient _http;
        private const string Endpoint = "https://api-inference.huggingface.co/models/sentence-transformers/meta-llama/Llama-3.1-8B-Instruct";
        private const string Token = "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";

        public HuggingFaceEmbeddingService(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var content = new StringContent($"\"{text}\"", Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(
                "https://api-inference.huggingface.co/models/sentence-transformers/meta-llama/Llama-3.1-8B-Instruct",
                content
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var embedding = JsonSerializer.Deserialize<List<List<float>>>(json);
            return embedding?[0]?.ToArray() ?? [];
        }

    }
}
