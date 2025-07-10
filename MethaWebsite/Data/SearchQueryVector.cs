namespace MethaWebsite.Data
{
    public class SearchQueryVector
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Query { get; set; }
        public float[]? Embedding { get; set; }
        public float Norm { get; set; }

    }
}
