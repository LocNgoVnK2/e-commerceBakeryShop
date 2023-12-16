using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class BusinessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
