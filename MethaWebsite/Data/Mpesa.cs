using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data
{
    public class Mpesa : Payment
    {
        [Required(ErrorMessage = "The Registered name field is required")]
        public string? RegisteredName { get; set; }
        public bool DefaultPaymentMethod { get; set; }
    }
}
