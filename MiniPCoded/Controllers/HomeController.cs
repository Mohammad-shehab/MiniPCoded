using System.Diagnostics;
using System.Transactions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MiniPCoded.Data;
using MiniPCoded.Models;
using MiniPCoded.Models.ViewModels;


namespace MPBankMiniProject.Controllers
{
    public class HomeController : Controller
    {
        #region InjectedServices
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private ApplicationDbContext db;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, ApplicationDbContext _applicatioDbContext, IWebHostEnvironment hostEnvironment)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            db = _applicatioDbContext;
            webHostEnvironment = hostEnvironment;

        }

        #endregion

        public async Task<IActionResult> Index()
        {
            // ADD view Model
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            IndexViewModel model = new IndexViewModel()
            {
                Balance = user.Balance,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicturePath = user.ProfilePicturePath,
                Id = user.Id
            };


            return View(model);
        }


        public async Task<IActionResult> AccountDetails()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new IndexViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Gender = (IndexViewModel.Genders)user.Gender,
                CivilID = user.CivilID,
                Balance = user.Balance,
                AccountNumber = user.AccountNumber,
                ProfilePicturePath = user.ProfilePicturePath
            };

            return View(model);
        }

        public async Task<IActionResult> AllUsers()
        {
            var currentUser = await userManager.GetUserAsync(User);

            var users = userManager.Users
                .Where(user => user.Id != currentUser.Id)
                .Select(user => new IndexViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Balance = user.Balance,
                    ProfilePicturePath = user.ProfilePicturePath
                }).ToList();

            return View(users);
        }




        public async Task<IActionResult> EditAccountDetails()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) { return NotFound(); }

            EditViewModel model = new EditViewModel()
            {
                Balance = user.Balance,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicturePath = user.ProfilePicturePath,
                Id = user.Id
            };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> EditAccountDetails(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                if (model.ProfilePicturePath != null)
                {
                    string pic = UploadFile(model.ProfilePicturePath);
                    user.ProfilePicturePath = pic;
                }

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("AccountDetails");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

            }
            return View(model);
        }

        private string UploadFile(object newImg)
        {
            throw new NotImplementedException();
        }



        [HttpGet]
        public async Task<IActionResult> Allusers()
        {
            var curr = await userManager.GetUserAsync(User);
            var users = userManager.Users.Where(user => user.Id != curr.Id)
            .Select(user => new IndexViewModel
            {
                Balance = user.Balance,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicturePath = user.ProfilePicturePath,
                Id = user.Id
            }).ToList();

            return View(users);
        }
        //public async Task<IActionResult> TransactionsList()
        //{
        //    var curr = await userManager.GetUserAsync(User);
        //    var transactions = db.Transactions.Where(tran => tran.ApplicationUserId == curr.Id)
        //    .Select(tran => new TransactionViewModel
        //    {
        //        Amount = tran.Amount,
        //        TransactionDate = tran.TransactionDate,
        //        Type = (TransactionViewModel.TransactionType)tran.Type
        //    }).ToList();
        //    return View(transactions);
        //}


    }
}
