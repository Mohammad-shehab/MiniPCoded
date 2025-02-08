using CPCoded.Models;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CPCoded.Data;
using CPCoded.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;



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
        #region GeneralActions
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AllApplications()
        {
            return View(db.LoanApplications.Where(x=>x.Status==LoanApplication.LoanStatus.Pending
            ));
        }
        public async Task<IActionResult> ApprovedApp()
        {
            return View(db.LoanApplications.Where(x => x.Status == LoanApplication.LoanStatus.Approved
            ));
        }
        public async Task<IActionResult> RejectedApp()
        {
            return View(db.LoanApplications.Where(x => x.Status == LoanApplication.LoanStatus.Rejected
            ));
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

        public async Task<IActionResult> Reject(int id)
        {
            var loan = await db.LoanApplications.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            if (loan.Status == LoanApplication.LoanStatus.Pending)
            {
                loan.Status = LoanApplication.LoanStatus.Rejected;

                var user = await userManager.FindByIdAsync(loan.ApplicationUserId);
                if (user != null)
                {
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
        #endregion
        #region AdminActions


        public IActionResult AddAdmin1()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddAdmin1(RegisterStep1ViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["Email"] = model.Email;
                TempData["Password"] = model.Password;
                TempData["ConfirmPassword"] = model.ConfirmPassword;
                return RedirectToAction("AddAdmin2");
            }

            return View(model);
        }

        
        public IActionResult AddAdmin2()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin2(RegisterStep2ViewModel model, IFormFile profilePicture)
        {
            if (ModelState.IsValid)
            {
                var email = TempData["Email"]?.ToString();
                var password = TempData["Password"]?.ToString();

                if (email == null || password == null)
                {
                    ModelState.AddModelError(string.Empty, "Registration step 1 data is missing.");
                    return View(model);
                }

                string imagePath = null;
                if (profilePicture != null)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var filePath = Path.Combine(uploadsDir, profilePicture.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePicture.CopyToAsync(stream);
                    }
                    imagePath = profilePicture.FileName;
                }

                ApplicationUser user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    Gender = (ApplicationUser.Genders)model.Gender,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    CivilID = model.CivilID,
                    Balance = 0,
                    AccountNumber = ApplicationUser.GenerateAccountNumber(),
                    ProfilePicturePath = imagePath // Add profile picture path
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> TrackRepayments()
        {
            var repayments = await db.LoanRepayments
                .Include(r => r.LoanApplication)
                .ToListAsync();

            return View(repayments);
        }

        public async Task<IActionResult> OverdueLoans()
        {
            var overdueLoans = await db.LoanApplications
                .Where(l => l.Status == LoanApplication.LoanStatus.Pending && l.ApplicationDate.AddMonths((int)l.DurationInMonths) < DateTime.Now)
                .ToListAsync();

            return View(overdueLoans);
        }
        #endregion


        public async Task<IActionResult> AllUsers()
        {
            var usersInRole = await userManager.GetUsersInRoleAsync("User");
            return View(usersInRole);
        }

        public async Task<IActionResult> AllAdmins()
        {
            var adminsInRole = await userManager.GetUsersInRoleAsync("Admin");
            return View(adminsInRole);
        }

        public async Task<IActionResult> AllPaymentsHistory()
        {
            var payments = await db.LoanRepayments
                .Include(r => r.LoanApplication)
                .ThenInclude(l => l.ApplicationUser)
                .ToListAsync();
            return View(payments);
        }


        public async Task<IActionResult> ProfileDetails()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public async Task<IActionResult> UserDetails(string id)
{
    var user = await userManager.FindByIdAsync(id);
    if (user == null)
    {
        return NotFound();
    }

    var loans = await db.LoanApplications
        .Where(l => l.ApplicationUserId == id)
        .ToListAsync();

    var model = new UserDetailsViewModel
    {
        User = user,
        Loans = loans
    };

    return View(model);
}

        public async Task<IActionResult> LoanDetails(int id)
{
    var loan = await db.LoanApplications
        .Include(l => l.LoanRepayments)
        .FirstOrDefaultAsync(l => l.Id == id);

    if (loan == null)
    {
        return NotFound();
    }

    return View(loan);
}


    }
}
