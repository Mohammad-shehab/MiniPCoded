using CPCoded.Data;
using CPCoded.Models;
using CPCoded.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CPCoded.Controllers
{
    [Authorize(Roles = "User")]
    public class LoanController : Controller
    {
        #region InjectedServices
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext db;

        public LoanController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, ApplicationDbContext _applicatioDbContext)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            db = _applicatioDbContext;
        }
        #endregion
        #region UserLoanActions
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var loanApplications = await db.LoanApplications
                .Where(loan => loan.ApplicationUserId == user.Id)
                .ToListAsync();

            return View(loanApplications.Where(x => x.Status == LoanApplication.LoanStatus.Approved));
        }

        public async Task<IActionResult> ApplyLoan()
        {
            var currentUser = await userManager.GetUserAsync(User);

            // Add Type enum values to ViewBag
            ViewBag.Types = Enum.GetValues(typeof(LoanApplication.Type))
                                .Cast<LoanApplication.Type>()
                                .Select(t => new SelectListItem
                                {
                                    Value = t.ToString(),
                                    Text = t.ToString()
                                }).ToList();

            // Add Duration enum values to ViewBag
            ViewBag.Durations = Enum.GetValues(typeof(LoanApplication.Duration))
                                    .Cast<LoanApplication.Duration>()
                                    .Select(d => new SelectListItem
                                    {
                                        Value = ((int)d).ToString(),
                                        Text = ((int)d).ToString()
                                    }).ToList();

            if (currentUser == null)
            {
                return NotFound();
            }

            var model = new LoanApplicationViewModel
            {
                ApplicationUserId = currentUser.Id // Set ApplicationUserId to the current user's ID
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyLoan(LoanApplicationViewModel model)
        {
            var currentUser = await userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                // Re-populate ViewBag in case of validation errors
                ViewBag.Types = Enum.GetValues(typeof(LoanApplication.Type))
                                    .Cast<LoanApplication.Type>()
                                    .Select(t => new SelectListItem
                                    {
                                        Value = t.ToString(),
                                        Text = t.ToString()
                                    }).ToList();

                ViewBag.Durations = Enum.GetValues(typeof(LoanApplication.Duration))
                                        .Cast<LoanApplication.Duration>()
                                        .Select(d => new SelectListItem
                                        {
                                            Value = ((int)d).ToString(),
                                            Text = ((int)d).ToString()
                                        }).ToList();

                return View(model);
            }

            LoanApplication loan = new LoanApplication
            {
                ApplicationDate = model.ApplicationDate,
                ApplicationUserId = currentUser.Id, // Ensure ApplicationUserId is set to the current user's ID
                DurationInMonths = model.Duration,
                LoanAmount = model.LoanAmount,
                Status = model.Status,
                LoanType = model.LoanType
            };

            db.LoanApplications.Add(loan);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        #region PaymentActions
        public async Task<IActionResult> PaymentHistory()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var payments = await db.LoanRepayments
                .Include(r => r.LoanApplication)
                .Where(r => r.LoanApplication.ApplicationUserId == user.Id)
                .ToListAsync();

            return View(payments);
        }
        public async Task<IActionResult> RepayLoan(int? id)
        {
            var loan = await db.LoanApplications.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            var model = new LoanRepaymentViewModel
            {
                LoanApplicationId = loan.Id,
                RemainingBalance = loan.LoanAmount - db.LoanRepayments.Where(r => r.LoanApplicationId == loan.Id).Sum(r => r.AmountPaid)
            };

            // Generate monthly payments
            var monthlyAmount = model.RemainingBalance / ((int)loan.DurationInMonths);
            for (int i = 1; i <= ((int)loan.DurationInMonths); i++)
            {
                model.MonthlyPayments.Add(new MonthlyPayment
                {
                    Month = i,
                    DueDate = loan.ApplicationDate.AddMonths(i),
                    Amount = monthlyAmount
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RepayLoan(LoanRepaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loan = await db.LoanApplications.FindAsync(model.LoanApplicationId);
            if (loan == null)
            {
                return NotFound();
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var totalPaid = db.LoanRepayments.Where(r => r.LoanApplicationId == loan.Id).Sum(r => r.AmountPaid);
            var remainingBalance = loan.LoanAmount - totalPaid;

            if (model.PayFull)
            {
                if (model.AmountPaid != remainingBalance)
                {
                    ModelState.AddModelError("", "Payment amount must be equal to the remaining balance for full payment.");
                    return View(model);
                }
            }
            else
            {
                if (model.AmountPaid > remainingBalance)
                {
                    ModelState.AddModelError("", "Payment amount exceeds remaining balance.");
                    return View(model);
                }
            }

            if (user.Balance < (float)model.AmountPaid)
            {
                ModelState.AddModelError("", "Insufficient balance.");
                return View(model);
            }

            user.Balance -= (float)model.AmountPaid;
            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user balance.");
                return View(model);
            }

            var repayment = new LoanRepayment
            {
                LoanApplicationId = model.LoanApplicationId,
                AmountPaid = model.AmountPaid,
                PaymentDate = DateTime.Now,
                TransactionReference = Guid.NewGuid().ToString() // Generate a unique transaction reference
            };

            db.LoanRepayments.Add(repayment);
            await db.SaveChangesAsync();

            // Update loan status if fully paid
            if (remainingBalance - model.AmountPaid <= 0)
            {
                loan.Status = LoanApplication.LoanStatus.PaidOff;
                db.LoanApplications.Update(loan);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Loan");
        }


        #endregion
    }
}
