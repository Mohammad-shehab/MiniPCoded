using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CPCoded.Models
{
    public class LoanRepayment
    {
        public int Id { get; set; }

        [Required]
        public int LoanApplicationId { get; set; }

        [ForeignKey("LoanApplicationId")]
        public LoanApplication LoanApplication { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be a positive value.")]
        public double AmountPaid { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string TransactionReference { get; set; }
    }
}
