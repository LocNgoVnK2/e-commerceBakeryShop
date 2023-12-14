using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class DetailController : Controller
    {
        private readonly IProductsService _productsService;
        private readonly ICategoryService _categoryService;
        private readonly IRateService _rateService;
        private readonly IReviewService _reviewService;
        private readonly IMapper _mapper;

        public DetailController(IProductsService productsService,
                                ICategoryService categoryService,
                                IRateService rateService,
                                IReviewService reviewService,
                                IMapper mapper)
        {
            _productsService = productsService;
            _categoryService = categoryService;
            _rateService = rateService;
            _reviewService = reviewService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index(int id)
        {
            
            Product product = await _productsService.GetProduct(id);
            List<Review> reviews = new List<Review>();
            ProductViewModel productViewModel = _mapper.Map<ProductViewModel>(product);
            if (product != null)
            {
                try
                {
                    IQueryable<Rate> rates = await _rateService.GetRatesByProductID((int)productViewModel.ProductID);
                    foreach (Rate rate in rates)
                    {
                        Review review = await _reviewService.GetReviewsByRatingId((int)rate.RatingId);
                        if (review != null) 
                            reviews.Add(review);
                    }
                    float starAverage = (float)rates.Average(rate => rate.Star);
                    productViewModel.Star = (starAverage % 1 >= 0.5) ? (int)starAverage + 1 : (int)starAverage;
                    productViewModel.NumberRate = rates.Count();
                    productViewModel.Reviews = reviews;
                    return View(productViewModel);
                }
                catch
                {
                 
                    productViewModel.Star = 5;
                    return View(productViewModel);
                }
              
            }
            else
            {
                return NotFound(); 
            }
            
        }

    }
}
