using Google;
using MethaWebsite.Data;
using MethaWebsite.Data.Contexts;

namespace MethaWebsite.Services
{
    public class AlgoliaIndexer
    {
        private readonly HttpClient _client;
        private readonly string appId = "2Y4YO3YY8F";
        private readonly string adminApiKey = "e606db3c656f681d7eedab472b84d39b";
        private readonly ApplicationDbContext applicationDbContext;

        public AlgoliaIndexer(HttpClient client, ApplicationDbContext appDbContext)
        {
            _client = client;
            _client.DefaultRequestHeaders.Add("X-Algolia-API-Key", adminApiKey);
            _client.DefaultRequestHeaders.Add("X-Algolia-Application-Id", appId);
            applicationDbContext = appDbContext;
        }

        public async Task<bool> IndexProductsAsync(IEnumerable<Product> products)
        {
            var payload = new
            {
                requests = products.Select(p => new
                {
                    action = "addObject",
                    body = new
                    {
                        objectID = p.Id,
                        name = p.Name,
                        description = p.Description,
                        price = p.Price,
                        category = applicationDbContext.Category.FirstOrDefault(c => c.Id == p.CategoryId)
        }
                })
            };

            var response = await _client.PostAsJsonAsync(
                $"https://{appId}.algolia.net/1/indexes/products/batch", payload);

            return response.IsSuccessStatusCode;
        }
    }
}
