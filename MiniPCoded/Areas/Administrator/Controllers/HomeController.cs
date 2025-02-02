using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace MiniPCoded.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        [Area("Administrator")]

        public IActionResult Index()
        {
            return View();
        }
    }
}
