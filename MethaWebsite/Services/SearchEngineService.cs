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
        public async Task<List<Product>> SearchAsync(string query, List<Product> products, double threshold = 0.55)
        {
            var queryVector = await _embeddingService.GetEmbeddingAsync(query);

            return products
                .Select(p => new
                {
                    Product = p,
                    Score = CosineSimilarity(queryVector, p.Embedding)
                })
                .Where(x => x.Score >= threshold)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Product)
                .ToList();
        }
        public List<Product> RecommendSimilar(Product target, List<Product> allProducts, double threshold = 0.75, int max = 35)
        {
            return allProducts
                .Where(p => p.Id != target.Id && p.Color != target.Color && p.Embedding?.Length > 0)
                .Select(p => new
                {
                    Product = p,
                    Score = CosineSimilarity(target.Embedding, p.Embedding)
                })
                .Where(x => x.Score >= threshold)
                .OrderByDescending(x => x.Score)
                .Take(max)
                .Select(x => x.Product)
                .ToList();
        }
        public static double CosineSimilarity(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length != b.Length || a.Length == 0)
                return 0;

            double dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            double denominator = Math.Sqrt(magA) * Math.Sqrt(magB);
            return denominator == 0 ? 0 : dot / denominator;
        }
    }
}
