using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CPCoded.Models;

namespace CPCcoded.Models.ViewModels
{
    public class LoanApplicationViewModel
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [Required(ErrorMessage = "Type amount")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Loan amount must be a positive value.")]
        [Display(Name = "Loan Amount")]
        public float LoanAmount { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [Required]
        public LoanApplication.LoanStatus Status { get; set; } = LoanApplication.LoanStatus.Pending;

        [Required]
        public LoanApplication.Duration Duration { get; set; }

        [Required]
        public LoanApplication.Type LoanType { get; set; }
    }
}
