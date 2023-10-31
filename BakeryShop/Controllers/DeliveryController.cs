using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class DeliveryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
