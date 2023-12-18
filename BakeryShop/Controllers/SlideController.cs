using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using iText.IO.Image;
using Microsoft.AspNetCore.Mvc;

namespace BakeryShop.Controllers
{
    public class SlideController : Controller
    {
        private readonly ISlideService _slideService;
        private readonly IMapper _mapper;
        public SlideController( ISlideService slideService, IMapper mapper) {
            _slideService = slideService;
            _mapper = mapper;
        }
        public async Task<IActionResult> AddSlideAsync(SlideViewModel model)
        {

            try
            {
                if (model.ImageData != null && model.ImageData.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageData.CopyToAsync(memoryStream);
                        model.Image = memoryStream.ToArray();
                    }
                }

                Slide slide = _mapper.Map<Slide>(model);
                await _slideService.InsertSlide(slide);
                return RedirectToAction("SlideManagement", "DashBoard");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        public async Task<IActionResult> DeleteSlide(int id)
        {
            try
            {
                await _slideService.DeleteSlide(id);
                return RedirectToAction("SlideManagement", "DashBoard");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public async Task<IActionResult> EditSlide(SlideViewModel model)
        {

            try
            {
                if (model.ImageData != null && model.ImageData.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImageData.CopyToAsync(memoryStream);
                        model.Image = memoryStream.ToArray();
                    }
                }
                if(model.ImageData == null)
                {
                    var temp = await _slideService.GetSlide((int)model.SlideID);
                    model.Image=temp.Image;
                }
                Slide slide = _mapper.Map<Slide>(model);
                await _slideService.UpdateSlide(slide);
                return RedirectToAction("SlideManagement", "DashBoard");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
