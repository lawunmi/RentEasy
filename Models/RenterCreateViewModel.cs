namespace RentEasy.Models
{
    public class RenterCreateViewModel
    {
        public String ItemId { get; set; }
        public String ItemTitle { get; set; }

        public String ItemDescription { get; set; }
        public DateTime RentStartDate { get; set; }
        public DateTime RentEndDate { get; set; }

        public List<string> ItemImageUrls { get; set; } = new List<string>();

        public List<ItemDropdownViewModel> AvailableItems { get; set; } = new List<ItemDropdownViewModel>();
    }
}
