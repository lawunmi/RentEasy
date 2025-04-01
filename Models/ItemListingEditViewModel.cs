using System.ComponentModel.DataAnnotations;

namespace RentEasy.Models
{
    public class ItemListingEditViewModel
    {
        public String ItemId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }

        [Required]
        public List<string> ExistingImages { get; set; } = new List<string>();

    }
}

