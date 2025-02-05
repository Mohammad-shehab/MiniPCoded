using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CPCoded.Models.ViewModels
{
    public class LoanRepaymentViewModel
    {
        public int LoanApplicationId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be a positive value.")]
        public double AmountPaid { get; set; }

        public string? TransactionReference { get; set; }

        public double RemainingBalance { get; set; }

        public bool PayFull { get; set; }

        public List<MonthlyPayment> MonthlyPayments { get; set; } = new List<MonthlyPayment>();
    }

    public class MonthlyPayment
    {
        public int Month { get; set; }
        public DateTime DueDate { get; set; }
        public double Amount { get; set; }
    }
}

