using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MiniPCoded.Models
{
    public class Transaction
    {

        [Display(Name = "Transaction ID")]
        public Guid TransactionId { get; set; }




        [Required]
        [Range(0.01, 1000000000, ErrorMessage = "Amount is High.")]
        public float Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public TransactionType Type { get; set; }
        public enum TransactionType { Transfer, Deposit, Withdrawal }



        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }


    }
}
