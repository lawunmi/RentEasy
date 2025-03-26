using Microsoft.EntityFrameworkCore;
using RentEasy.Models;

namespace RentEasy.Data
{
    public class RentEasyContext : DbContext
    {
        public RentEasyContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Itemlisting> ItemListing { get; set; }
        public DbSet<Booking> Bookings { get; set; }
    }
}
