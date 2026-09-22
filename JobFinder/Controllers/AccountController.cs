using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
