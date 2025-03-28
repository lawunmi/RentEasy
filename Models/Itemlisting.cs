﻿using System.ComponentModel.DataAnnotations;

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
        public float PricePerDay { get; set; }
        public float PricePerWeek { get; set; }
        public float PricePerMonth { get; set; }
        public List<String> ItemImages { get; set; }


    }
}
