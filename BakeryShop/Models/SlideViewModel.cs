namespace BakeryShop.Models
{
    public class SlideViewModel
    {
        public int? SlideID { get; set; }
        public IFormFile? ImageData { get; set; }

        public byte[]? Image { get; set; }
        public int? Precedence { get; set; }
    }
}
