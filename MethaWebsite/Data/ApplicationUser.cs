using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MethaWebsite.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100, ErrorMessage = "The {0} field cannot be empty.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string Name { get; set; } = "";
        public byte[]? ProfilePicture { get; set; }
        public string? DefaultAddressId { get; set; }
        public string? ShoppingCardId { get; set; }
    }

}
