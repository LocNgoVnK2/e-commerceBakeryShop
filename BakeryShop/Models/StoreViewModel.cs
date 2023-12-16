namespace BakeryShop.Models
{
    public class StoreViewModel
    {
        public int? IdStore { get; set; }
        public string? Address { get; set; }
        public string? StoreName { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public bool? IsUsed { get; set; }
    }
}
