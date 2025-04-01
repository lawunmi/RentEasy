namespace RentEasy.Models
{
    public class RenterCreateViewModel
    {
        public string ItemId { get; set; }
        public string? ItemDescription { get; set; }
        public List<Itemlisting> AvailableItems { get; set; } = new List<Itemlisting>();  // Ensure it's a list
        public DateTime RentStartDate { get; set; }
        public DateTime RentEndDate { get; set; }
    }
}
