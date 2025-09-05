namespace MethaWebsite.Data
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Subject { get; set; }
        public string? Comment { get; set; }
        public bool Read { get; set; } = false;
        public DateTime? Created { get; set; } = DateTime.Now;
        public string? RecipientId { get; set; }
        public List<string>? ReplyIds { get; set; }
    }
}
