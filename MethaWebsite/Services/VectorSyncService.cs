using MethaWebsite.Data.Contexts;
using OpenAI;
using OpenAI.Embeddings;
using Pinecone;
using System;
using System.Linq;

namespace MethaWebsite.Services
{
    public class VectorSyncService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private EmbeddingService EmbeddingService { get; set; }

        public VectorSyncService(IServiceProvider services, EmbeddingService embeddingService)
        {
            _services = services;
            EmbeddingService = embeddingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var pinecone = scope.ServiceProvider.GetRequiredService<IndexClient>();
            var openai = scope.ServiceProvider.GetRequiredService<EmbeddingClient>();

            var products = db.Product.ToList();
            var batch = new List<Vector>();

            foreach (var product in products)
            {
                var input = $"{product.Name} {product.Description}";

                var embeddingResponse = await EmbeddingService.GenerateAsync(input);

                var vector = new Vector
                {
                    Id = product.Id,
                    Values = embeddingResponse.ToArray(),
                    Metadata = new Metadata
                    {
                        ["name"] = product.Name,
                        ["description"] = product.Description
                    }
                };

                batch.Add(vector);
            }

            if (batch.Any())
            {
                await pinecone.UpsertAsync(new UpsertRequest
                {
                    Vectors = batch
                });
            }
        }
    }
}
