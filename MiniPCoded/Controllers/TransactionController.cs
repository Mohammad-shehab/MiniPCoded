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
        private readonly IWebHostEnvironment webHostEnvironment;

        public TransactionController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, ApplicationDbContext _applicatioDbContext, IWebHostEnvironment hostEnvironment)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            db = _applicatioDbContext;
            webHostEnvironment = hostEnvironment;

        }

        #endregion

        public async Task<IActionResult> TransactionsList()
        {
            var mylist = await userManager.GetUserAsync(User);
            var transactions = db.Transactions.Where(user => user.ApplicationUserId == mylist.Id)
            .Select(user => new TransactionViewModel
            {
                Amount = user.Amount,
                TransactionDate = user.TransactionDate,
                Type = (TransactionViewModel.TransactionType)user.Type
            }).ToList();
            return View(transactions);
        }


        public async Task<IActionResult> Transaction()
        {
            var r = await userManager.GetUserAsync(User);
            if (r == null) { return NotFound(); }
            ViewBag.Balance = r.Balance;
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
                ModelState.AddModelError("Insufficient", "You cannot withdraw more than the available balance.");
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            Models.Transaction transaction = new Models.Transaction()
            {
                Amount = model.Amount,
                Type = (choice == "Withdraw") ? Models.Transaction.TransactionType.Withdrawal : Models.Transaction.TransactionType.Deposit,
                TransactionDate = model.TransactionDate,
                ApplicationUserId = user.Id
            };

            db.Transactions.Add(transaction);
            db.SaveChanges();

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
            var transferie = await userManager.FindByIdAsync(model.TargetId);
            if (transferie == null)
            {
                ModelState.AddModelError("UserNotFound", "User Not Found");
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            if (user.Balance - model.Amount < 0)
            {
                ModelState.AddModelError("Insufficient", "You cannot transfer more than the available balance.");
                ViewBag.Balance = user.Balance;
                return View(model);
            }

            Models.Transaction tran = new Models.Transaction()
            {
                Amount = model.Amount,
                Type = Models.Transaction.TransactionType.Transfer,
                TransactionDate = DateTime.Now,
                ApplicationUserId = user.Id
            };

            db.Transactions.Add(tran);
            db.SaveChanges();

            user.Balance -= model.Amount;
            transferie.Balance += model.Amount;

            var result = await userManager.UpdateAsync(user);
            var result2 = await userManager.UpdateAsync(transferie);

            if (result.Succeeded && result2.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            ViewBag.Balance = user.Balance;
            return View(model);
        }




        public async Task<IActionResult> TransactionsList2(string transactionType, float? minAmount, float? maxAmount, DateTime? startDate, DateTime? endDate)
        {
            var user = await userManager.GetUserAsync(User);
            var query = db.Transactions.Where(t => t.ApplicationUserId == user.Id);

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.Type.ToString() == transactionType);
            }
            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }
            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }
            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= endDate.Value);
            }

            var transactions = await query
                .Select(t => new TransactionViewModel
                {
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Type = (TransactionViewModel.TransactionType)t.Type
                }).ToListAsync();

            return View(transactions);
        }



    }
}
