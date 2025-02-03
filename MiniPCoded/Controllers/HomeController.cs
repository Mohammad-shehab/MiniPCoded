using System.Diagnostics;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using CPCoded.Data;
using CPCoded.Models;
using CPCoded.Models.ViewModels;


namespace CPCoded.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        #region InjectedServices
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private ApplicationDbContext db;
        public HomeController(UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, ApplicationDbContext _applicatioDbContext)
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



        public async Task<IActionResult> EditAccountDetails()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) 
            { return NotFound(); 
            }

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
        public async Task<IActionResult> EditAccountDetails(EditViewModel model, IFormFile profilePicture)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retain the current profile picture path if no new picture is uploaded
            string imagePath = user.ProfilePicturePath;
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate a unique file name to avoid overwriting existing files
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);
                var filePath = Path.Combine(uploadsDir, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }
                imagePath = uniqueFileName;
            }

            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfilePicturePath = imagePath; // Use the retained or updated profile picture path

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }



        //public async Task<IActionResult> Allusers()
        //{
        //    var curr = await userManager.GetUserAsync(User);
        //    var users = userManager.Users.Where(user => user.Id != curr.Id)
        //    .Select(user => new IndexViewModel
        //    {
        //        Balance = user.Balance,
        //        Email = user.Email,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        ProfilePicturePath = user.ProfilePicturePath,
        //        Id = user.Id
        //    }).ToList();

        //    return View(users);
        //}
  

    }
}
