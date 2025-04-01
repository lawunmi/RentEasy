using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentEasy.Data;
using RentEasy.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentEasy.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class APIController : ControllerBase
    {
        private readonly RentEasyContext _reDbContext;
        private readonly IAmazonS3 _s3Client;
        private readonly string bucketName = "s3-renteasy"; 

        public APIController(RentEasyContext reDbContext, IAmazonS3 s3Client)
        {
            _reDbContext = reDbContext;
            _s3Client = s3Client;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool userExists = await _reDbContext.Users.AnyAsync(u => u.Username == model.Username);
            if (userExists)
            {
                return BadRequest(new { message = "Username or Email already in use." });
            }

            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                Role = model.Role
            };

            _reDbContext.Users.Add(user);
            await _reDbContext.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }

        // Create an Item Listing (with Image Upload)
        [HttpPost("itemlisting")]
        public async Task<IActionResult> CreateItem([FromForm] Itemlisting model, [FromForm] List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure S3 bucket exists
            bool bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists)
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                });
            }

            // Upload images to S3
            var imageUrls = new List<string>();
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    string imageUrl = await UploadToS3(image);
                    imageUrls.Add(imageUrl);
                }
            }

            model.ItemId = Guid.NewGuid().ToString();
            model.ItemImages = imageUrls;

            _reDbContext.ItemListing.Add(model);
            await _reDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = model.ItemId }, model);
        }

        // Get All Item Listings
        [HttpGet("itemlisting")]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _reDbContext.ItemListing.ToListAsync();
            return Ok(items);
        }

        //  Get Item by ID
        [HttpGet("itemlisting/{id}")]
        public async Task<IActionResult> GetItemById(string id)
        {
            var item = await _reDbContext.ItemListing.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }
            return Ok(item);
        }

        // Update Item Listing
        [HttpPut("itemlisting/{id}")]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] Itemlisting model)
        {
            var item = await _reDbContext.ItemListing.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            item.Title = model.Title;
            item.Description = model.Description;
            item.AvailableFrom = model.AvailableFrom;
            item.AvailableTo = model.AvailableTo;
            item.PricePerDay = model.PricePerDay;
            item.PricePerWeek = model.PricePerWeek;
            item.PricePerMonth = model.PricePerMonth;

            await _reDbContext.SaveChangesAsync();
            return Ok(new { message = "Item updated successfully" });
        }

        // Delete Item Listing
        [HttpDelete("itemlisting/{id}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            var item = await _reDbContext.ItemListing.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            _reDbContext.ItemListing.Remove(item);
            await _reDbContext.SaveChangesAsync();
            return Ok(new { message = "Item deleted successfully" });
        }

        // Upload Image to AWS S3
        private async Task<string> UploadToS3(IFormFile file)
        {
            string fileKey = $"items/{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileKey,
                BucketName = bucketName,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return $"https://{bucketName}.s3.amazonaws.com/{fileKey}";
        }

        // Get all listings
        [HttpGet("listings")]
        public async Task<IActionResult> GetListings()
        {
            var listings = await _reDbContext.ItemListing.ToListAsync();
            return Ok(listings);
        }

        // Get a single listing by ID
        [HttpGet("listings/{id}")]
        public async Task<IActionResult> GetListing(string id)
        {
            var listing = await _reDbContext.ItemListing.FindAsync(id);
            if (listing == null)
                return NotFound("Listing not found");

            return Ok(listing);
        }

        // Create a new listing
        [HttpPost("listings")]
        public async Task<IActionResult> CreateListing([FromBody] Itemlisting model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                model.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _reDbContext.ItemListing.Add(model);
                await _reDbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetListing), new { id = model.ItemId }, model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error creating listing: " + ex.Message);
            }
        }

        // Update a listing
        [HttpPut("listings/{id}")]
        public async Task<IActionResult> UpdateListing(string id, [FromBody] Itemlisting model)
        {
            var existingListing = await _reDbContext.ItemListing.FindAsync(id);
            if (existingListing == null)
                return NotFound("Listing not found");

            existingListing.Title = model.Title;
            existingListing.Description = model.Description;
            existingListing.AvailableFrom = model.AvailableFrom;
            existingListing.AvailableTo = model.AvailableTo;
            existingListing.PricePerDay = model.PricePerDay;
            existingListing.PricePerWeek = model.PricePerWeek;
            existingListing.PricePerMonth = model.PricePerMonth;

            await _reDbContext.SaveChangesAsync();
            return NoContent();
        }

        // Delete a listing
        [HttpDelete("listings/{id}")]
        public async Task<IActionResult> DeleteListing(string id)
        {
            var listing = await _reDbContext.ItemListing.FindAsync(id);
            if (listing == null)
                return NotFound("Listing not found");

            _reDbContext.ItemListing.Remove(listing);
            await _reDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("rentals")]
        public async Task<IActionResult> GetRentals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rentals = await _reDbContext.Bookings
                                .Where(b => b.BookerId == userId)
                                .Include(b => b.Itemlisting)
                                .ToListAsync();

            return Ok(rentals);
        }

        // Create a rental booking
        [HttpPost("rentals")]
        public async Task<IActionResult> CreateRental([FromBody] Booking model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                model.BookerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                model.Status = "Pending";

                _reDbContext.Bookings.Add(model);
                await _reDbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetRentals), new { id = model.BookingID }, model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error creating rental: " + ex.Message);
            }
        }

        // Delete a booking
        [HttpDelete("rentals/{id}")]
        public async Task<IActionResult> DeleteRental(string id)
        {
            var booking = await _reDbContext.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound("Rental not found");

            _reDbContext.Bookings.Remove(booking);
            await _reDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
