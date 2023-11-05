using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class RatingProduct : Controller
    {

        public RatingProduct() { 

     
        }
        public IActionResult Index(int orderId)
        {
            // for theo ma sp r load vào 
            return View();
        }
        
        public async Task<IActionResult> AddRating( ReviewProductViewModel model) { 
        
            return RedirectToAction("ReviewOrder","Cart");
        }
        
    }
}
