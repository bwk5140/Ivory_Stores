using MethaWebsite.Data;
using System.Text;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class AlgoliaService
    {
        private readonly HttpClient _httpClient;
        private readonly string appId = "2Y4YO3YY8F";
        private readonly string apiKey = "cbd13f67ad128cfd155d30f5f6c96489";
        private const string Endpoint = "https://insights.algolia.io/1/events";

        public AlgoliaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("X-Algolia-API-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Algolia-Application-Id", appId);
        }

        public async Task<List<Product>> SearchProductsAsync(string query)
        {
            var json = new
            {
                query = query,
                hitsPerPage = 50,
                clickAnalytics = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://{appId}-dsn.algolia.net/1/indexes/products/query", json);

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
                var queryId = jsonDoc.RootElement.GetProperty("queryID").GetString();
                return jsonDoc.RootElement.GetProperty("hits")
                    .EnumerateArray()
                    .Select(hit => new Product
                    {
                        Name = hit.GetProperty("name").GetString(),
                        Description = hit.GetProperty("description").GetString(),
                        Price = hit.GetProperty("price").GetDouble(),
                        CategoryId = queryId
                    })
                    .ToList();
            }

            return [];
        }
        public async Task<List<Recommendation>> GetRecommendationsAsync(string objectId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Algolia-Application-Id", appId);
            client.DefaultRequestHeaders.Add("X-Algolia-API-Key", apiKey);

            var requestUri = $"https://recommendation.algolia.com/1/models/frequently-bought-together/recommendations?indexName=products_index&objectID={objectId}";

            var response = await client.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var recommendations = JsonSerializer.Deserialize<RecommendationResponse>(json);
                return recommendations.Hits;
            }
            return new List<Recommendation>();
        }
        public async Task SendEventAsync(string eventType, string eventName, string indexName, string userToken, string[] objectIDs,int[] positions, string? queryID = null)
        {
            var payload = new
            {
                events = new[]
                {
                new
                {
                    eventType,
                    eventName,
                    index = indexName,
                    userToken,
                    objectIDs,
                    positions,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    queryID
                }
            }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { IgnoreNullValues = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(Endpoint, content);
            response.EnsureSuccessStatusCode();
        }

    }
}
