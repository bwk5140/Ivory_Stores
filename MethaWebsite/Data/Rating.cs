namespace MethaWebsite.Data
{
    public class Rating
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool Selected { get; set; }
    }
}
