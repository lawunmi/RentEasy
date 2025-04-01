using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using RentEasy.Data;
using RentEasy.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RentEasy.Controllers
{
    public class ListingController : Controller
    {
        private readonly RentEasyContext _reDbContext;
        private readonly IAmazonS3 _s3Client;
        private readonly string bucketName = "s3-renteasy";

        public ListingController(RentEasyContext reDbContext, IAmazonS3 s3Client)
        {
            _reDbContext = reDbContext;
            _s3Client = s3Client;
        }

        // View All Items
        public async Task<IActionResult> Index()
        {
            var items = await _reDbContext.ItemListing.ToListAsync();
            return View(items);
        }

        // Show Create Form
        public IActionResult Create()
        {
            return View(new ItemListingCreateViewModel());
        }

        // Create Item (Handles Image Upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemListingCreateViewModel model)
        {
            // Print all validation errors to help debug
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Field '{state.Key}' has errors:");
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                }
            }

            // Handle images manually since we're having issues with binding
            var images = Request.Form.Files.Count > 0 ? Request.Form.Files : null;

            if (images == null || images.Count == 0)
            {
                ModelState.AddModelError("Images", "At least one image is required");
                return View(model);
            }

            // Check other required fields
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check all required fields.";
                return View(model);
            }

            try
            {
                // Create the Itemlisting object with the correct casing to match your model
                var item = new Itemlisting
                {
                    OwnerId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    Title = model.Title,
                    Description = model.Description ?? "",
                    AvailableFrom = model.AvailableFrom,
                    AvailableTo = model.AvailableTo,
                    PricePerDay = model.PricePerDay,
                    PricePerWeek = model.PricePerWeek,
                    PricePerMonth = model.PricePerMonth,
                    ItemImages = new List<string>()
                };

                // Ensure the S3 bucket exists
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
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        if (image != null && image.Length > 0)
                        {
                            string imageUrl = await UploadToS3(image);
                            item.ItemImages.Add(imageUrl);
                        }
                    }
                }

                // Add and save to database
                _reDbContext.ItemListing.Add(item);
                await _reDbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Item created successfully!";
                return RedirectToAction("Index", "OwnerDashboard");
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database-specific errors
                TempData["ErrorMessage"] = "Database error: " + (dbEx.InnerException?.Message ?? dbEx.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                // Handle general errors
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return View(model);
            }
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _reDbContext.ItemListing
                                         .Where(i => i.ItemId == id && i.OwnerId == userId)
                                         .FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound(); 
            }

            var model = new ItemListingEditViewModel
            {
                ItemId = item.ItemId,
                Title = item.Title,
                Description = item.Description,
                AvailableFrom = item.AvailableFrom,
                AvailableTo = item.AvailableTo,
                PricePerDay = item.PricePerDay,
                PricePerWeek = item.PricePerWeek,
                PricePerMonth = item.PricePerMonth,
                ExistingImages = item.ItemImages.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemListingEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _reDbContext.ItemListing
                                         .Where(i => i.ItemId == model.ItemId && i.OwnerId == userId)
                                         .FirstOrDefaultAsync();

            if (item == null)
            {
                TempData["ErrorMessage"] = "Item not found or you don't have permission to edit it.";
                return RedirectToAction("Index", "OwnerDashboard");
            }

            // Update fields
            item.Title = model.Title;
            item.Description = model.Description;
            item.AvailableFrom = model.AvailableFrom;
            item.AvailableTo = model.AvailableTo;
            item.PricePerDay = model.PricePerDay;
            item.PricePerWeek = model.PricePerWeek;
            item.PricePerMonth = model.PricePerMonth;

            // Handle new image uploads (if any)
            // Handle new image uploads (if any)
            var images = Request.Form.Files;
            if (images.Count > 0)
            {
                var newImageUrls = new List<string>();
                foreach (var image in images)
                {
                    string imageUrl = await UploadToS3(image);
                    newImageUrls.Add(imageUrl);
                }
                // Append new images to existing images
                item.ItemImages.AddRange(newImageUrls);
            }

            await _reDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item updated successfully!";
            return RedirectToAction("Index", "OwnerDashboard");
        }


        //Delete Item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _reDbContext.ItemListing
                                         .Where(i => i.ItemId == id && i.OwnerId == userId)
                                         .FirstOrDefaultAsync();

            if (item == null)
            {
                TempData["ErrorMessage"] = "Item not found or you don't have permission to delete it.";
                return RedirectToAction("Index", "OwnerDashboard");
            }

            try
            {
                //Delete images from S3 if any exist
                if (item.ItemImages != null && item.ItemImages.Count > 0)
                {
                    foreach (var imageUrl in item.ItemImages)
                    {
                        await DeleteFromS3(imageUrl);
                    }
                }

                //Remove the item from the database
                _reDbContext.ItemListing.Remove(item);
                await _reDbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Item deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting item: " + ex.Message;
            }

            return RedirectToAction("Index", "OwnerDashboard");
        }

        //Delete Image from AWS S3
        private async Task DeleteFromS3(string imageUrl)
        {
            try
            {
                //Extract the file key from the full S3 URL
                var fileKey = imageUrl.Replace($"https://{bucketName}.s3.amazonaws.com/", "");

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete image from S3: {ex.Message}");
            }
        }

    }
}