using System.ComponentModel.DataAnnotations;

namespace RentEasy.Models
{
    public class User
    {
        [Key]
        public String UserId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
