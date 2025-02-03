using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPCoded.Controllers
{
    [Authorize(Roles = "User")]
    public class LoanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
