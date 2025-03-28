using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;
using RentEasy.Models;
using System.Security.Claims;

namespace RentEasy.Controllers
{
    public class OwnerDashboardController : Controller
    {
        private readonly RentEasyContext _reDbContext;

        public OwnerDashboardController(RentEasyContext reDbContext)
        {
            _reDbContext = reDbContext;
        }
        public async Task<IActionResult> Index()
        {
            var items = await _reDbContext.ItemListing.Where(r => r.ItemId == User.Identity.Name).ToListAsync();
            return View(items);  
        }

        //public async Task<IActionResult> Index()
        //{
        //    var rentals = await _reDbContext.ItemListing
        //        .Where(r => r.ItemId == User.Identity.Name)
        //        .ToListAsync();

        //    if (rentals == null)
        //    {
        //        rentals = new List<ItemListing>();  // Avoid passing null to the view
        //    }

        //    return View(rentals);
        //}

    }
}
