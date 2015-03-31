using System.ComponentModel.DataAnnotations;

namespace Tracker.Models.Account
{
    public class RegisterModel
    {
        [Required]
#if !PORTABLE
        [EmailAddress]
#else
        [DataType(DataType.EmailAddress)]
#endif
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}
