using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniPCoded.Data;
using MiniPCoded.Models;
using MiniPCoded.Models.ViewModels;




namespace MiniPCoded.Controllers
{
    [Authorize]

    public class TransactionController : Controller
    {

        #region InjectedServices
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private ApplicationDbContext db;

        public TransactionController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, ApplicationDbContext _applicatioDbContext)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            db = _applicatioDbContext;

        }

        #endregion

        public async Task<IActionResult> Transaction()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound(); 
            }
            ViewBag.Balance = user.Balance;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Transaction(TransactionViewModel model, string choice)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            if (choice == "Withdraw" && (user.Balance - model.Amount) < 0)
            {
                ModelState.AddModelError("InsufficientFunds", "Insufficient funds to complete this transaction.");
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            var transactionType = (choice == "Withdraw")
                ? Models.Transaction.TransactionType.Withdrawal
                : Models.Transaction.TransactionType.Deposit;

            var transaction = new Models.Transaction
            {
                Amount = model.Amount,
                Type = transactionType,
                TransactionDate = model.TransactionDate,
                ApplicationUserId = user.Id
            };

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            user.Balance += (choice == "Withdraw") ? -model.Amount : model.Amount;
            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                ViewBag.Balance = user.Balance;
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            ViewBag.Balance = user.Balance;
            return View(model);
        }

       
        public async Task<IActionResult> Transfer(string? id)
        {
            var user = await userManager.GetUserAsync(User);

            TransferViewModel transfer = new TransferViewModel()
            {
                Id = user.Id,
                TargetId = id,
                Amount = 0
            };
            ViewBag.Balance = user.Balance;
            return View(transfer);
        }


        [HttpPost]
        public async Task<IActionResult> Transfer(TransferViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            var transfer = await userManager.FindByIdAsync(model.TargetId);

            if (transfer == null)
            {
                ModelState.AddModelError("UserNotFound", "The specified user could not be located.");
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            if (user.Balance < model.Amount)
            {
                ModelState.AddModelError("InsufficientFunds", "Insufficient balance for this transfer.");
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            var transaction = new Models.Transaction
            {
                Amount = model.Amount,
                Type = Models.Transaction.TransactionType.Transfer,
                TransactionDate = DateTime.Now,
                ApplicationUserId = user.Id
            };

            db.Transactions.Add(transaction);
            db.SaveChanges();

            user.Balance -= model.Amount;
            transfer.Balance += model.Amount;

            var UserResult = await userManager.UpdateAsync(user);
            var updateTransferResult = await userManager.UpdateAsync(transfer);

            if (UserResult.Succeeded && updateTransferResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in UserResult.Errors.Concat(updateTransferResult.Errors))
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            ViewBag.Balance = user.Balance;
            return View(model);
        }


        public async Task<IActionResult> TransactionsList2(float? minAmount, float? maxAmount, DateTime? startDate, DateTime? endDate)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transactionsQuery = db.Transactions
                .Where(t => t.ApplicationUserId == user.Id)
                .AsQueryable();

            if (minAmount.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.Amount <= maxAmount.Value);
            }

            if (startDate.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                transactionsQuery = transactionsQuery.Where(t => t.TransactionDate <= endDate.Value);
            }



            var transactions = await transactionsQuery
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return View(transactions);
        }


        }
}
