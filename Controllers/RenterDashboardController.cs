using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace RentEasy.Controllers
{
    public class RenterDashboardController : Controller
    {
        private readonly RentEasyContext _reDbContext;

        public RenterDashboardController(RentEasyContext reDbContext)
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

            // Fetch only rented-items owned by the logged-in user
            var renterBookings = await _reDbContext.Bookings
                .Include(b => b.Itemlisting)  // Ensure this relationship is properly set up
                .Where(b => b.BookerId == userId)
                .ToListAsync();

            if (renterBookings == null || !renterBookings.Any())
            {
                TempData["InfoMessage"] = "No items found for your account.";
            }

            return View(renterBookings);
        }
    }
}
