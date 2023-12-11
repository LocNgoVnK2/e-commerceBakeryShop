namespace BakeryShop.Models
{
    public class ReviewProductViewModel
    {
        public int? ReviewID { get; set; }
        public int? ProductID { get; set; }
        public int? StarNumber { get; set; }
        public string? ProductName { get; set; }
        public string? DisplayName { get; set; }
        public string? ReviewContent { get; set; }

        public string? RateStatus { get; set; }
    }
}
