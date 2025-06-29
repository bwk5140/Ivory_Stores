using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data
{
    public class Address
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string? FullName { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public bool DefaultAddress { get; set; }
        [Required]
        public string? UserId { get; set; }
    }
}
