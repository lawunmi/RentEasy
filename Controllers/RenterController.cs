using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;
using RentEasy.Models;

namespace RentEasy.Controllers
{
    public class RenterController : Controller
    {
        private readonly RentEasyContext _reDbContext;

        public RenterController(RentEasyContext reDbContext)
        {
            _reDbContext = reDbContext;
        }
        public async Task<IActionResult> Create()
        {
            var availableItems = await _reDbContext.ItemListing
                .Select(i => new ItemDropdownViewModel
                {
                    ItemId = i.ItemId,
                    Title = i.Title
                })
                .ToListAsync();

            var model = new RenterCreateViewModel
            {
                AvailableItems = availableItems
            };

            return View(model);
        }


        [HttpGet("GetItemDetails/{itemId}")]
        public async Task<IActionResult> GetItemDetails(string itemId)
        {
            var item = await _reDbContext.ItemListing
                .Where(i => i.ItemId == itemId)
                .Select(i => new
                {
                    ItemTitle = i.Title,
                    ItemDescription = i.Description,
                    ItemImages = i.ItemImages // Assuming this is stored as List<string>
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

    }
}
