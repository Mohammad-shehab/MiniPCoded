using System.ComponentModel.DataAnnotations;

namespace CPCoded.Models.ViewModels
{
    public class TransactionViewModel
    {



        [Required]
        [Range(0.01, 1000000000, ErrorMessage = "Amount is High.")]
        public float Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public TransactionType Type { get; set; }
        public enum TransactionType { Transfer, Deposit, Withdrawal }

    


    }
}
