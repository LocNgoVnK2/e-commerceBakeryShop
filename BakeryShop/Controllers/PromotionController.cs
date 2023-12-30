using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class PromotionController : Controller
    {   
        private readonly IPromotionService _promotionService;
        private readonly IMapper _mapper;
        public PromotionController(IPromotionService promotionService, IMapper mapper)
        {
            _promotionService = promotionService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddPromotion(PromotionViewModel promotionViewModel)
        {
                try
                {
               
                    Promotion promotion = _mapper.Map<Promotion>(promotionViewModel);

                    await _promotionService.InsertPromotion(promotion);

 
                    return RedirectToAction("PromotionManagement", "DashBoard");
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu cần thiết
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm khuyến mãi. Vui lòng thử lại sau.");
                }

            return RedirectToAction("PromotionManagement", "DashBoard");
        }
        [HttpPost]
        public async Task<IActionResult> EditPromotion(PromotionViewModel promotionViewModel)
        {
            try
            {
                Promotion promotion = _mapper.Map<Promotion>(promotionViewModel);
                await _promotionService.UpdatePromotion(promotion);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi sửa đổi khuyến mãi. Vui lòng thử lại sau.");
            }

            return RedirectToAction("PromotionManagement", "DashBoard");
        }
        public async Task<IActionResult> DeletePromotion(int id)
        {
            try
            {
                Promotion existingPromotion = await _promotionService.GetPromotion(id);

                if (existingPromotion != null)
                {
                    await _promotionService.DeletePromotion(existingPromotion);

                    return RedirectToAction("PromotionManagement", "DashBoard");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy khuyến mãi để xóa.");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa khuyến mãi. Vui lòng thử lại sau.");
            }

            return RedirectToAction("PromotionManagement", "DashBoard");
        }
    }

}
