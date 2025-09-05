namespace MethaWebsite.Data
{
    public class Country
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
