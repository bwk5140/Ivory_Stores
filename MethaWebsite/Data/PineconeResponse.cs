namespace MethaWebsite.Data
{
    public class PineconeResponse
    {
        public List<Match> Matches { get; set; }
    }

    public class Match
    {
        public string Id { get; set; }
        public float Score { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
