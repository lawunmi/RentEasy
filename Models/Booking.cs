using System.ComponentModel.DataAnnotations;

namespace RentEasy.Models
{
    public class Booking
    {
        public String BookingID { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public int ItemId { get; set; }

        public int BookerId { get; set; } // The user creating the booking

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public float TotalAmount { get; set; }

        public string Status { get; set; }
    }
}
