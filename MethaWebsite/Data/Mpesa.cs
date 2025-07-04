using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data
{
    public class Mpesa : Payment
    {
        [Required (ErrorMessage = "The Phone number field is required")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "The Registered name field is required")]
        public string? RegisteredName { get; set; }
        public bool DefaultPaymentMethod { get; set; }
    }
}
