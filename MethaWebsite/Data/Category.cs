namespace MethaWebsite.Data
{
    public class Category
    {
        public string? Name { get; set; }
        public string? Id { get; set; } = Guid.NewGuid().ToString();
        public string? ParentID { get; set; }
    }
}
