using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPCoded.Models
{
    public class LoanApplication
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "Type amount")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Loan amount must be a positive value.")]
        [Display(Name = "Loan Amount")]
       
        public float LoanAmount { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive integer.")]
        public int DurationInMonths { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [Required]
        public LoanStatus Status { get; set; } = LoanStatus.Pending;
        public enum LoanStatus { Pending, Approved, Rejected }

        //===========================================================================
        [Required]
        [ForeignKey("ApplicationUserId")]
        public string UserId { get; set; }  //FK
        public ApplicationUser? ApplicationUserId { get; set; }
    }
}