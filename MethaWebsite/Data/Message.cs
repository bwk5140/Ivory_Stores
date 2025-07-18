namespace MethaWebsite.Data
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Subject { get; set; }
        public string? Comment { get; set; }
        public DateTime? Created { get; set; }
        public List<string>? ReplyIds { get; set; }
    }
}
