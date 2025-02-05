using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CPCoded.Models
{
    public class LoanApplication
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Type amount")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Loan amount must be a positive value.")]
        [Display(Name = "Loan Amount")]
        public float LoanAmount { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive integer.")]
        public Duration DurationInMonths { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [Required]
        public LoanStatus Status { get; set; } = LoanStatus.Pending;

        public enum LoanStatus { Pending, Approved, Rejected,
            PaidOff
        }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public enum Duration : int
        {
            TwelveMonths = 12,
            TwentyFourMonths = 24,
            ThirtySixMonths = 36,
            FortyEightMonths = 48,
            SixtyMonths = 60
        }

        [Required]
        public Type LoanType { get; set; }

        public enum Type
        {
            Housing, Consumer, Auto, Medical
        }
    }
}
