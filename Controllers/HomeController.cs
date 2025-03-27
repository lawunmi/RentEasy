using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentEasy.Data;
using RentEasy.Models;

namespace RentEasy.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly RentEasyContext _reDbContext;

        public HomeController(RentEasyContext reDbContext)
        {
            _reDbContext = reDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
