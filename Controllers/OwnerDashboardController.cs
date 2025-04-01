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
            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to access the dashboard.";
                return RedirectToAction("Login", "Account");
            }

            // Fetch only items owned by the logged-in user
            var items = await _reDbContext.ItemListing
                                          .Where(r => r.OwnerId == userId)
                                          .ToListAsync();

            if (items == null || !items.Any())
            {
                TempData["InfoMessage"] = "No items found for your account.";
            }

            return View(items);
        }
    }
}
