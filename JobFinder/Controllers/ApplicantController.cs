using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    public class ApplicantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
