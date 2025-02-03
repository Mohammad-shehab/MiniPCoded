using CPCoded.Data;
using CPCoded.Models;
using CPCoded.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MiniPCoded.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CPCoded.Controllers
{
    [Authorize(Roles = "User")]
    public class LoanController : Controller
    {
        #region InjectedServices
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private ApplicationDbContext db;
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
                .Select(loan => new LoanApplicationViewModel
                {
                    Id = loan.Id,
                    ApplicationUserId = loan.ApplicationUserId,
                    UserId = loan.ApplicationUserId,
                    LoanAmount = loan.LoanAmount,
                    //DurationInMonths = (LoanApplicationViewModel.Duration)user.Durations,
                    ApplicationDate = loan.ApplicationDate,
                })
                .ToListAsync();

            return View(loanApplications);
        }

        public async Task<IActionResult> ApplyLoan()
        {
            var currentUser = await userManager.GetUserAsync(User);
            

            if (currentUser == null)
            {
                return NotFound();           
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ApplyLoan(LoanApplicationViewModel model)
        {
            var currentUser = await userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            LoanApplication loan = new LoanApplication
            {
                ApplicationDate = model.ApplicationDate,
                ApplicationUserId = model.ApplicationUserId,
                DurationInMonths = (LoanApplication.Duration)model.DurationInMonths,
                LoanAmount = model.LoanAmount,
                Status = LoanApplication.LoanStatus.Pending
            };


            db.LoanApplications.Add(loan);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}

