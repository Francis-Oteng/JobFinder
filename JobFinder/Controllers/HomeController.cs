using JobFinder.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobFinder.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
