using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using BakeryShop.Utils;
using System.Transactions;

namespace BakeryShop.Controllers
{
    public class RatingProductController : Controller
    {
        private readonly IOrderDetailService _orderDetailService;
        private readonly IOrderService _orderService;
        private readonly IRateService _rateService;
        private readonly IReviewService _reviewService;
        private readonly IProductsService _productsService;
        public RatingProductController( IReviewService reviewService,
                               IRateService rateService,
                               IOrderService orderService,
                               IOrderDetailService orderDetailService,
                               IProductsService productsService) { 
            _orderDetailService = orderDetailService;
            _rateService = rateService;
            _reviewService = reviewService;
            _orderService = orderService;   
            _productsService = productsService;
     
        }
        public async Task<IActionResult> Index(int orderId)
        {
            IQueryable<OrderDetail> orderDetails = await _orderDetailService.GetOrderDetailsByOrderId( orderId );
            IQueryable<Product> ProductList = await _productsService.GetProducts();

            List<ReviewProductViewModel> reviews = new List<ReviewProductViewModel>();

            foreach ( OrderDetail orderDetail in orderDetails )
            {
                ReviewProductViewModel review = new ReviewProductViewModel()
                {
                    
                    ProductID = orderDetail.ProductID,
                    ProductName = ProductList.FirstOrDefault(e=>e.ProductID== orderDetail.ProductID)?.ProductName
                };
                reviews.Add(review);

            }
            // for theo ma sp r load vào 
            return View(reviews);
        }
        
        public async Task<IActionResult> AddRating( ReviewProductViewModel model) { 
        

            return RedirectToAction("ReviewOrder","Cart");
        }

         [HttpPost]
         public async Task<IActionResult> ProcessReview([FromBody] ReviewProductViewModel model)
         {
             if (Utils.Utils.ContainsObsceneWords(model.DisplayName) && Utils.Utils.ContainsObsceneWords(model.ReviewContent)){
                return BadRequest("Review của bạn có chứ từ chửi tục nên nó đã bị xóa.");
             }
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    Rate rate = new Rate()
                    {
                        StatusRate = true,
                        Star = (int)model.StarNumber,
                        ProductId = (int)model.ProductID
                    };
                    await _rateService.InsertRate(rate);

                    int ratingId = (int)rate.RatingId;

                    Review review = new Review() { 
                        DisplayName = model.DisplayName,
                        ReviewContent = model.ReviewContent,
                        RatingId= ratingId
                    };
                    await _reviewService.InsertReview(review);
                    scope.Complete();
                    return Ok("Review submitted successfully");

                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return NotFound(ex.Message);
                }
            }
         }

    }
}
