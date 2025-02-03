using System.ComponentModel.DataAnnotations;

namespace CPCoded.Models.ViewModels
{
    public class EditViewModel
    {

       
            [Required(ErrorMessage = "Enter Your First Name")]
            [Display(Name = "First Name")]
            [MinLength(2, ErrorMessage = " At Least 1 Charachters")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            [Required(ErrorMessage = "Enter Your Last Name")]
            [MinLength(2, ErrorMessage = "At Least 1 Charachters")]
            public string LastName { get; set; }

            [Display(Name = "Email Address")]
            [Required(ErrorMessage = "Email is Required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid Email Address.")]
            [MinLength(6, ErrorMessage = "Email Address must be at least 6 characters long.")]
            public string Email { get; set; }

            [Required]
            public float Balance { get; set; }

            [Display(Name = "Profile Picture")]
            public string? ProfilePicturePath { get; set; }

            [Required]
            public string Id { get; set; }

        }
    }
