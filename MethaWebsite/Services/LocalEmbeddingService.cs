using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class LocalEmbeddingService
    {
        private readonly HttpClient _http;

        public LocalEmbeddingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var payload = new { text };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("http://localhost:5000/embed", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var vector = doc.RootElement.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
            return vector;
        }
    }
}
