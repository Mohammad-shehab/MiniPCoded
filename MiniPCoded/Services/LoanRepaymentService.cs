using CPCoded.Data;
using CPCoded.Models;
using CPCoded.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CPCoded.Services
{
    public class LoanRepaymentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoanRepaymentService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<LoanRepaymentViewModel> GetLoanRepaymentViewModelAsync(int loanApplicationId, ApplicationUser user)
        {
            // Securely fetch loan with ownership check
            var loan = await _dbContext.LoanApplications
                .FirstOrDefaultAsync(l => l.Id == loanApplicationId && l.ApplicationUserId == user.Id);

            if (loan == null)
            {
                return null;
            }

            var totalPaid = await _dbContext.LoanRepayments
                .Where(r => r.LoanApplicationId == loan.Id)
                .SumAsync(r => r.AmountPaid);

            var remainingBalance = loan.LoanAmount - totalPaid;

            var model = new LoanRepaymentViewModel
            {
                LoanApplicationId = loan.Id,
                RemainingBalance = remainingBalance,
                MinimumPayment = Math.Max(remainingBalance * 0.10, 50) // Ensure minimum is at least 50
            };

            // Generate monthly payments schedule
            var monthlyAmount = loan.LoanAmount / (int)loan.DurationInMonths;
            model.MonthlyPayments = Enumerable.Range(1, (int)loan.DurationInMonths)
                .Select(i => new MonthlyPayment
                {
                    Month = i,
                    DueDate = loan.ApplicationDate.AddMonths(i),
                    Amount = monthlyAmount,
                    IsPaid = _dbContext.LoanRepayments.Any(r => r.LoanApplicationId == loan.Id && r.PaymentDate.Month == loan.ApplicationDate.AddMonths(i).Month)
                }).ToList();

            return model;
        }

        public async Task<bool> ProcessRepaymentAsync(LoanRepaymentViewModel model, ApplicationUser user)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Securely fetch the loan
                var loan = await _dbContext.LoanApplications
                    .FirstOrDefaultAsync(l => l.Id == model.LoanApplicationId && l.ApplicationUserId == user.Id);

                if (loan == null)
                {
                    return false;
                }

                var totalPaid = await _dbContext.LoanRepayments
                    .Where(r => r.LoanApplicationId == loan.Id)
                    .SumAsync(r => r.AmountPaid);

                var remainingBalance = loan.LoanAmount - totalPaid;

                // Ensure the payment is valid
                if (model.PayFull && model.AmountPaid != remainingBalance)
                {
                    return false; // Full payment must match remaining balance exactly
                }
                else if (model.PayMinimum && model.AmountPaid != model.MinimumPayment)
                {
                    return false; // Minimum payment must match exactly
                }
                else if (model.AmountPaid > remainingBalance || model.AmountPaid <= 0)
                {
                    return false; // Prevent overpayment or negative values
                }

                // Ensure user has enough balance
                if (user.Balance < (float)model.AmountPaid)
                {
                    return false;
                }

                // Deduct from user balance
                user.Balance -= (float)model.AmountPaid;
                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    return false;
                }

                // Save repayment transaction
                var repayment = new LoanRepayment
                {
                    LoanApplicationId = model.LoanApplicationId,
                    AmountPaid = model.AmountPaid,
                    PaymentDate = DateTime.UtcNow,
                    TransactionReference = Guid.NewGuid().ToString()
                };

                _dbContext.LoanRepayments.Add(repayment);
                await _dbContext.SaveChangesAsync();

                // Update loan status if fully paid
                totalPaid += model.AmountPaid;
                if (totalPaid >= loan.LoanAmount)
                {
                    loan.Status = LoanApplication.LoanStatus.PaidOff;
                    _dbContext.LoanApplications.Update(loan);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync(); // Ensure everything saves together
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(); // Prevent partial updates
                return false;
            }
        }

        public async Task<bool> PayMonthlyDueAsync(int loanApplicationId, int month, double amount, ApplicationUser user)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Securely fetch the loan
                var loan = await _dbContext.LoanApplications
                    .FirstOrDefaultAsync(l => l.Id == loanApplicationId && l.ApplicationUserId == user.Id);

                if (loan == null)
                {
                    return false;
                }

                // Ensure user has enough balance
                if (user.Balance < (float)amount)
                {
                    return false;
                }

                // Deduct from user balance
                user.Balance -= (float)amount;
                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    return false;
                }

                // Save repayment transaction
                var repayment = new LoanRepayment
                {
                    LoanApplicationId = loanApplicationId,
                    AmountPaid = amount,
                    PaymentDate = DateTime.UtcNow,
                    TransactionReference = Guid.NewGuid().ToString()
                };

                _dbContext.LoanRepayments.Add(repayment);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync(); // Ensure everything saves together
                return true;
            }
            catch
            {
                await transaction.RollbackAsync(); // Prevent partial updates
                return false;
            }
        }
    }
}
