using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CPCoded.Models;
using CPCoded.Models.ViewModels;

namespace CPCoded.Controllers
{

    public class AccountController : Controller
    {
        #region Injected Services
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(SignInManager<ApplicationUser> _signInManager, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            roleManager = _roleManager;
        }
        #endregion

        #region Users

        [AllowAnonymous]
        public IActionResult RegisterStep1()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult RegisterStep1(RegisterStep1ViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["Email"] = model.Email;
                TempData["Password"] = model.Password;
                TempData["ConfirmPassword"] = model.ConfirmPassword;
                return RedirectToAction("RegisterStep2");
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult RegisterStep2()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RegisterStep2(RegisterStep2ViewModel model, IFormFile profilePicture)
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
                    await userManager.AddToRoleAsync(user, "User");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Home", new { area = "Administrator" });
                        }
                        else if (roles.Contains("User"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError("", "Invalid Email or Password");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion
    }
}
