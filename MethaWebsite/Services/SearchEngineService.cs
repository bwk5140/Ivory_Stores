using MethaWebsite.Data;
using MethaWebsite.Data.Contexts;

namespace MethaWebsite.Services
{
    public class SearchEngineService
    {
        private readonly ApplicationDbContext _context;
        private readonly LocalEmbeddingService _embeddingService;

        public SearchEngineService(ApplicationDbContext applicationDbContext, LocalEmbeddingService embeddingService)
        {
            _context = applicationDbContext;
            _embeddingService = embeddingService;
        }
        public List<Product> SearchWithScoring(string query, List<Product> products)
        {
            var tokens = query.ToLower().Split(' ');
            return products.Select(p =>
            {
                var category = _context.Category.FirstOrDefault(c => c.Id == p.CategoryId);
                int score = 0;
                foreach (var t in tokens)
                {
                    if (p.Name.ToLower().Contains(t)) score += 5;
                    if (category.Name.ToLower().Contains(t)) score += 3;
                    if (p.Description.ToLower().Contains(t)) score += 1;
                    if (p.Brand.ToLower().Contains(t)) score += 1;
                }
                return (Product: p, Score: score);
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Product)
            .ToList();
        }
        public async Task<List<Product>> SearchAsync(float[] queryVector, float queryNorm, List<Product> products, double threshold = 0.55)
        {
            return products
                .Select(p => new
                {
                    Product = p,
                    Score = CosineSimilarity(queryVector, queryNorm, p.Embedding, p.Norm)
                })
                .Where(x => x.Score >= threshold)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Product)
                .ToList();
        }
        public async Task<List<Product>> SearchAsync(string query, List<Product> products, double threshold = 0.55)
        {
            var result = await _embeddingService.GetEmbeddingAsync(query);
            var queryVector = result.Vector;
            var norm = result.Norm;

            return products
                .Select(p => new
                {
                    Product = p,
                    Score = CosineSimilarity(queryVector, norm, p.Embedding, p.Norm)
                })
                .Where(x => x.Score >= threshold)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Product)
                .ToList();
        }
        public List<Product> RecommendSimilar(Product target, List<Product> allProducts, double threshold = 0.70, int max = 35)
        {
            return allProducts
                .Where(p => p.Id != target.Id && p.Color != target.Color && p.Embedding?.Length > 0)
                .Select(p => new
                {
                    Product = p,
                    Score = CosineSimilarity(target.Embedding, target.Norm, p.Embedding, p.Norm)
                })
                .Where(x => x.Score >= threshold)
                .OrderByDescending(x => x.Score)
                .Take(max)
                .Select(x => x.Product)
                .ToList();
        }
        private float CosineSimilarity(float[] vec1, float norm1, float[] vec2, float norm2)
        {
            float dot = 0;
            for (int i = 0; i < vec1.Length; i++)
            {
                dot += vec1[i] * vec2[i];
            }

            return dot / (norm1 * norm2);
        }

    }
}
