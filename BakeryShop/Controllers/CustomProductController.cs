using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class CustomProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
