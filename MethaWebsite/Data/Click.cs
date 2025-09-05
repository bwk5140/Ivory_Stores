namespace MethaWebsite.Data
{
    public class Click
    {
        public string Id {  get; set; } = Guid.NewGuid().ToString();
        public int position { get; set; }
        public string? UserId { get; set; }
        public string? ProductId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
