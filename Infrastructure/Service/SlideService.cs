using Infrastructure.Entities;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public interface ISlideService
    {
        Task<IQueryable<Slide>> GetSlides();
        Task<Slide> GetSlide(int id);
        Task InsertSlide(Slide slide);
        Task UpdateSlide(Slide slide);
        Task DeleteSlide(int slideId);
    }

    // Service implementation for Slide
    public class SlideService : ISlideService
    {
        private readonly ISlideRepository _slideRepository;

        public SlideService(ISlideRepository slideRepository)
        {
            _slideRepository = slideRepository;
        }

        public async Task DeleteSlide(int slideId)
        {
            var slide = await _slideRepository.GetByIdAsync(slideId);
            if (slide != null)
            {
                await _slideRepository.DeleteAsync(slide);
            }
        }

        public async Task<Slide> GetSlide(int id)
        {
            return await _slideRepository.GetByIdAsync(id);
        }

        public async Task<IQueryable<Slide>> GetSlides()
        {
            return await Task.FromResult(_slideRepository.GetAll());
        }

        public async Task InsertSlide(Slide slide)
        {
            await _slideRepository.InsertAsync(slide);
        }

        public async Task UpdateSlide(Slide slide)
        {
            await _slideRepository.UpdateAsync(slide);
        }
    }
}
