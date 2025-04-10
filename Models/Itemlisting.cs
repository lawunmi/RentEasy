﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentEasy.Models
{
    public class Itemlisting
    {
        [Key]
        public String ItemId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public String OwnerId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public List<string> ItemImages { get; set; } = new List<string>();

        [ForeignKey("OwnerId")]
        public User User { get; set; }
    }
}
