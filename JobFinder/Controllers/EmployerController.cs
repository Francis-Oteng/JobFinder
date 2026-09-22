using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    public class EmployerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
