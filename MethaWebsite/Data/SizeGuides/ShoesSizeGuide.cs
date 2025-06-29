using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data.SizeGuides
{
    public class ShoesSizeGuide : SizeGuide
    {
        [Required]
        public string? BrandSize { get; set; }
        public string? UK_Size { get; set; }
        public string? US_Size { get; set; }
        public string? EU_Size { get; set; }
    }
}
