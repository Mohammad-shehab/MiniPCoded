using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiniPCoded.Models.ViewModels
{
    public class LoanApplicationViewModel
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public string UserId { get; set; }  //FK

        [Required(ErrorMessage = "Type amount")]
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



        public Duration duration{get; set;}
        public enum Duration 
        {
            TwelveMonths = 12,
            TwentyFourMonths = 24,
            ThirtySixMonths = 36,
            FortyEightMonths = 48,
            SixtyMonths = 60
        }
    }
}
