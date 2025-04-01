using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace RentEasy.Models
{
    public class ItemListingCreateViewModel
    {

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }

        [Required]
        public List<IFormFile> Images { get; set; }
    }
}
