using MethaWebsite.Data;
using System.Text.Json;

namespace MethaWebsite.Services
{
    public class AlgoliaService
    {
        private readonly HttpClient _httpClient;
        private readonly string appId = "2Y4YO3YY8F";
        private readonly string apiKey = "cbd13f67ad128cfd155d30f5f6c96489";

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
                hitsPerPage = 50
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://{appId}-dsn.algolia.net/1/indexes/products/query", json);

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
                return jsonDoc.RootElement.GetProperty("hits")
                    .EnumerateArray()
                    .Select(hit => new Product
                    {
                        Name = hit.GetProperty("name").GetString(),
                        Description = hit.GetProperty("description").GetString(),
                        Price = hit.GetProperty("price").GetDouble()
                    })
                    .ToList();
            }

            return [];
        }
    }
}
