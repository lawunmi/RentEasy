using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;

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
            var items = await _reDbContext.Bookings.Where(r => r.BookingID == User.Identity.Name).ToListAsync();
            return View(items);
        }
    }
}
