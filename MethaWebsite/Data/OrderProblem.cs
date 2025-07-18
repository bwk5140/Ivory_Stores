namespace MethaWebsite.Data
{
    public class OrderProblem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public List<string>? MessageIds { get; set; }
        public List<string>? Subjects { get; set; }
    }
}
