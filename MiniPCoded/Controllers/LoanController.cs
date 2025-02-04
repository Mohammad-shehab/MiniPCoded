using CPCoded.Data;
using CPCoded.Models;
using CPCoded.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CPCcoded.Models.ViewModels;

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

            return View(loanApplications);
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




    }
}
