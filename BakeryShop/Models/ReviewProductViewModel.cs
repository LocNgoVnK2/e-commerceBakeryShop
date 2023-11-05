namespace BakeryShop.Models
{
    public class ReviewProductViewModel
    {
        public int? RatingId { get; set; }
        public int? ProductID { get; set; }
        public string? ProductName { get; set; }
        public string? Comment { get; set; }
    }
}
