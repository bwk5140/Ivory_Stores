using MethaWebsite.Data;
using MethaWebsite.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MethaWebsite.Controllers
{
    public class ProductSearchController : Controller
    {
        private readonly EmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly IProductRepository _productRepo;

        public ProductSearchController(
            EmbeddingService embeddingService,
            IVectorStore vectorStore,
            IProductRepository productRepo)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _productRepo = productRepo;
        }

        [HttpPost("/semantic-search")]
        public async Task<List<Product>> SemanticSearch([FromBody] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Product>();

            var embedding = await _embeddingService.GenerateAsync(query);
            var results = await _vectorStore.SearchAsync(embedding, topK: 10);
            return _productRepo.GetByIds(results.Select(r => r.Id));
        }

    }
}
