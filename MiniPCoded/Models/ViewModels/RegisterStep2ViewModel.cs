using System;
using System.ComponentModel.DataAnnotations;

namespace MiniPCoded.Models.ViewModels
{
    public class RegisterStep2ViewModel
    {
        public IFormFile? ProfilePicture { get; set; }

        [Required(ErrorMessage = "Enter First Name")]
        [MinLength(1)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Enter Last Name")]
        [MinLength(1)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Select Gender")]
        public Genders Gender { get; set; }
        public enum Genders { Male, Female }

        [Required(ErrorMessage = "Enter Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Enter Civil ID")]
        [MinLength(12)]
        [MaxLength(12)]
        public string CivilID { get; set; }


    }
}
