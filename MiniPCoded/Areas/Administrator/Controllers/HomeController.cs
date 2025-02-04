using CPCoded.Models;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CPCoded.Data;
using CPCoded.Models.ViewModels;
using System.Threading.Tasks;



namespace CPCoded.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    [Authorize(Roles = "Admin")]
   
    public class HomeController : Controller
    {
        #region Injected Services
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext db;

        public HomeController(SignInManager<ApplicationUser> _signInManager, UserManager<ApplicationUser> _userManager,ApplicationDbContext _applicationDbContext)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            db = _applicationDbContext;
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AllApplications()
        {
            return View(db.LoanApplications);
        }





           
        public async Task<IActionResult> AccountDetails(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var loan = await db.LoanApplications.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            if (loan.Status == LoanApplication.LoanStatus.Pending)
            {
                loan.Status = LoanApplication.LoanStatus.Approved;

                var user = await userManager.FindByIdAsync(loan.ApplicationUserId);
                if (user != null)
                {
                    user.Balance += loan.LoanAmount;
                    var result = await userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(loan);
                    }
                }

                db.LoanApplications.Update(loan);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("AllApplications");
        }


    }
}
