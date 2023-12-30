using System.ComponentModel.DataAnnotations;

namespace BakeryShop.Models
{
    public class PromotionViewModel
    {
        public int? PromotionID { get; set; }
        public string? PromotionName { get; set; }
        public string? Description { get; set; }
        public int? DiscountPercentage { get; set; }
        public Double? Condition { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
