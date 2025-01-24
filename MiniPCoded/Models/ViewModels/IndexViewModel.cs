using System.ComponentModel.DataAnnotations;

namespace MiniPCoded.Models.ViewModels
{
    public class IndexViewModel
    {


 

            [Required(ErrorMessage = "Please Enter Your First Name")]
            [Display(Name = "First Name")]
            [MinLength(1, ErrorMessage = "Name should be at least 1 character")]
             public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            [Required(ErrorMessage = "Please Enter Your Last Name")]
            [MinLength(1, ErrorMessage = "Name should be at least 1 character")]
            public string LastName { get; set; }

            [Display(Name = "Email Address")]
            [Required(ErrorMessage = "Email is Required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid Email Address.")]
            [MinLength(6, ErrorMessage = "Email Address must be at least 6 characters long.")]
            public string Email { get; set; }

            [Required]
            public float Balance { get; set; }

            public string? ProfilePicturePath { get; set; }

            [Required]
            public string Id { get; set; }



        [Required(ErrorMessage = "Select Gender")]
        public Genders Gender { get; set; }
        public enum Genders { Male, Female }












        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Account number must be a positive integer.")]
        public int AccountNumber { get; set; } = GenerateAccountNumber();








        // Method to generate a unique account number
        public static int GenerateAccountNumber()
        {
            var random = new Random();
            return random.Next(1, int.MaxValue);
        }





        









        [Required(ErrorMessage = "Enter Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }







        [Required(ErrorMessage = "Enter Civil ID")]
        [MinLength(12)]
        [MaxLength(12)]
        public string CivilID { get; set; }
        public string? UserName { get; internal set; }

       
    }
}



