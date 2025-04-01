using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentEasy.Models
{
    public class Booking
    {
        [Key]
        public String BookingID { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public String ItemId { get; set; }

        public String BookerId { get; set; } // The user creating the booking

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        [ForeignKey("BookerId")]
        public User User { get; set; }

        [ForeignKey("ItemId")]
        public Itemlisting Itemlisting { get; set; }

    } 
}
