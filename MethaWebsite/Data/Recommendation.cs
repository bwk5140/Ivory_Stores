namespace MethaWebsite.Data
{
    public class Recommendation
    {
        public string ObjectID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }

    public class RecommendationResponse
    {
        public List<Recommendation> Hits { get; set; }
    }
}
