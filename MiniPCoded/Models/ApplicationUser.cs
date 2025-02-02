using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace MiniPCoded.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required(ErrorMessage = "Select Gender")]
        public Genders Gender { get; set; }
        public enum Genders { Male, Female }








        [Required]
        public float Balance { get; set; } = 0;







        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Account number must be a positive integer.")]
        public int AccountNumber { get; set; } = GenerateAccountNumber();








        // Method to generate a unique account number
        public static int GenerateAccountNumber()
        {
            var random = new Random();
            return random.Next(1, int.MaxValue);
        }





        [Required(ErrorMessage = "Enter First Name")]
        [MinLength(1)]
        public string FirstName { get; set; }






        [Required(ErrorMessage = "Enter Last Name")]
        [MinLength(1)]
        public string LastName { get; set; }





        [Required(ErrorMessage = "Enter Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }







        [Required(ErrorMessage = "Enter Civil ID")]
        [MinLength(12)]
        [MaxLength(12)]
        public string CivilID { get; set; }





        public string ProfilePicturePath { get; set; }




        public IList<Transaction>? Transactions { get; set; }


 

    }
}