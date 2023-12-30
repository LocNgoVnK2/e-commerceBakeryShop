namespace BakeryShop.Models
{
    public class PromotionMappingViewModel
    {
        public int? MappingID { get; set; }
        public int? PromotionID { get; set; }
        public int? OrderID { get; set; }
        public double? DiscountAmount { get; set; }
    }
}
