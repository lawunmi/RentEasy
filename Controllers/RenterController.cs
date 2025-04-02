using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;
using RentEasy.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace RentEasy.Controllers
{
    public class RenterController : Controller
    {
        private readonly RentEasyContext _reDbContext;

        public RenterController(RentEasyContext reDbContext)
        {
            _reDbContext = reDbContext;
        }

        // Show available items for rent
        public async Task<IActionResult> Create()
        {
            var model = new RenterCreateViewModel
            {
                AvailableItems = await _reDbContext.ItemListing.ToListAsync()
            };
            return View(model);
        }

        // Get item details 
        [HttpGet]
        public async Task<JsonResult> GetItemDetails(string id)
        {
            var item = await _reDbContext.ItemListing.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found" });
            }

            return Json(new
            {
                success = true,
                itemDescription = item.Description,
                itemImages = item.ItemImages ?? new List<string>()  // Handle null images
            });
        }

        // Process the rental request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RenterCreateViewModel model)
        {
            // Validate selected item
            var item = await _reDbContext.ItemListing.FindAsync(model.ItemId);
            if (item == null)
            {
                ModelState.AddModelError("ItemId", "Selected item does not exist.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableItems = await _reDbContext.ItemListing.ToListAsync();
                return View(model);
            }

            // Get logged-in user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to rent an item.";
                return RedirectToAction("Login", "Account");
            }

            // Calculate rental cost based on duration
            var days = (model.RentEndDate - model.RentStartDate).Days;
            if (days <= 0)
            {
                ModelState.AddModelError("RentEndDate", "End date must be after the start date.");
                model.AvailableItems = await _reDbContext.ItemListing.ToListAsync();
                return View(model);
            }

            decimal totalAmount = days * item.PricePerDay;

            // Create booking record
            var booking = new Booking
            {
                ItemId = model.ItemId,
                BookerId = userId,
                StartDate = model.RentStartDate,
                EndDate = model.RentEndDate,
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            try
            {
                _reDbContext.Bookings.Add(booking);
                await _reDbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item successfully rented!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error processing rental: " + ex.Message);
                model.AvailableItems = await _reDbContext.ItemListing.ToListAsync();
                return View(model);
            }

            return RedirectToAction("Index", "RenterDashboard");
        }

        // Index Action - Show bookings with item details
        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to access the dashboard.";
                return RedirectToAction("Login", "Account");
            }

            // Fetch bookings by the logged-in user
            var bookings = await _reDbContext.Bookings
                .Include(b => b.Itemlisting)  // Ensure this relationship is correct
                .Where(b => b.BookerId == userId)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                TempData["InfoMessage"] = "No items found for your account.";
            }

            return View(bookings);
        }
    }
}
