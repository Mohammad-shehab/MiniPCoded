using CPCoded.Data;
using CPCoded.Models;
using CPCoded.Models.ViewModels;
using CPCoded.Services;
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
        private readonly LoanRepaymentService loanRepaymentService;

        public LoanController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, ApplicationDbContext _applicatioDbContext, LoanRepaymentService _loanRepaymentService)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            db = _applicatioDbContext;
            loanRepaymentService = _loanRepaymentService;
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

        public async Task<IActionResult> PendingApp()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var loanApplications = await db.LoanApplications
                .Where(loan => loan.ApplicationUserId == user.Id)
                .ToListAsync();

            return View(loanApplications.Where(x => x.Status == LoanApplication.LoanStatus.Pending));
        }

        public async Task<IActionResult> RejectedApp()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var loanApplications = await db.LoanApplications
                .Where(loan => loan.ApplicationUserId == user.Id)
                .ToListAsync();

            return View(loanApplications.Where(x => x.Status == LoanApplication.LoanStatus.Rejected));
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
            if (id == null)
            {
                return NotFound();
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Return 401 if the user is not authenticated
            }

            // Securely fetch the loan ensuring it's owned by the logged-in user
            var loan = await db.LoanApplications
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicationUserId == user.Id);

            if (loan == null)
            {
                return NotFound(); // Avoid Unauthorized to prevent user enumeration
            }

            var model = await loanRepaymentService.GetLoanRepaymentViewModelAsync(id.Value, user);
            if (model == null)
            {
                return NotFound();
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

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Prevent unauthorized access
            }

            // Fetch the loan with proper ownership verification
            var loan = await db.LoanApplications
                .FirstOrDefaultAsync(l => l.Id == model.LoanApplicationId && l.ApplicationUserId == user.Id);

            if (loan == null)
            {
                return NotFound(); // Prevent unauthorized access & enumeration
            }

            // Pass only validated data to the service layer
            var success = await loanRepaymentService.ProcessRepaymentAsync(model, user);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to process repayment.");
                return View(model);
            }

            return RedirectToAction("Index", "Loan");
        }

        [HttpPost]
        public async Task<IActionResult> PayMonthlyDue(int loanApplicationId, int month, double amount)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Prevent unauthorized access
            }

            // Pass only validated data to the service layer
            var success = await loanRepaymentService.PayMonthlyDueAsync(loanApplicationId, month, amount, user);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to process monthly due payment.");
                return RedirectToAction("RepayLoan", new { id = loanApplicationId });
            }

            return RedirectToAction("RepayLoan", new { id = loanApplicationId });
        }

        public async Task<IActionResult> CancelLoan(int id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Return 401 if the user is not authenticated
            }

            // Securely fetch the loan ensuring it's owned by the logged-in user
            var loan = await db.LoanApplications
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicationUserId == user.Id);

            if (loan == null)
            {
                return NotFound(); // Avoid Unauthorized to prevent user enumeration
            }

            return View(loan);
        }

        [HttpPost]
        public async Task<IActionResult> CancelLoanConfirmed(int id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Return 401 if the user is not authenticated
            }

            // Securely fetch the loan ensuring it's owned by the logged-in user
            var loan = await db.LoanApplications
                .FirstOrDefaultAsync(l => l.Id == id && l.ApplicationUserId == user.Id);

            if (loan == null)
            {
                return NotFound(); // Avoid Unauthorized to prevent user enumeration
            }

            db.LoanApplications.Remove(loan);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        #endregion
    }
}
