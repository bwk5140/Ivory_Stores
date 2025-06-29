using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data.SizeGuides
{
    public class ClothingSizeGuide : SizeGuide
    {
        [Required]
        public string? BrandSize { get; set; }
        public string? WaistSize { get; set; }
        public string? HipSize { get; set; }
        public string? BustSize { get; set; }
        public string? Length { get; set; }
    }
}
