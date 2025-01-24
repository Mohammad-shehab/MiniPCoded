using System.ComponentModel.DataAnnotations;

namespace MiniPCoded.Models.ViewModels
{
    public class RegisterStep1ViewModel
    {
        [Required(ErrorMessage = "Enter Email Address")]
        [EmailAddress]
        [MinLength(6)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(12)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
