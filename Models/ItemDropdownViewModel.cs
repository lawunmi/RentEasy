namespace RentEasy.Models
{
    public class ItemDropdownViewModel
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
        public float PricePerDay { get; set; }
        public List<string> ItemImages { get; set; }
    }
}
