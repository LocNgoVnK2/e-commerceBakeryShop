using Infrastructure.Entities;
using Infrastructure.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public interface IPromotionMappingService
    {
        Task<IQueryable<PromotionMapping>> GetPromotionMappings();
        Task<PromotionMapping> GetPromotionMapping(int id);
        Task<IQueryable<PromotionMapping>> GetPromotionMappingsByOrderID(int id);
        Task InsertPromotionMapping(PromotionMapping promotionMapping);
        Task UpdatePromotionMapping(PromotionMapping promotionMapping);
        Task DeletePromotionMapping(PromotionMapping promotionMapping);
    }

    public class PromotionMappingService : IPromotionMappingService
    {
        private readonly IPromotionMappingRepository _promotionMappingRepository;

        public PromotionMappingService(IPromotionMappingRepository promotionMappingRepository)
        {
            _promotionMappingRepository = promotionMappingRepository;
        }

        public async Task DeletePromotionMapping(PromotionMapping promotionMapping)
        {

            await _promotionMappingRepository.DeleteAsync(promotionMapping);
        }

        public async Task<PromotionMapping> GetPromotionMapping(int id)
        {
            return await _promotionMappingRepository.GetByIdAsync(id);
        }

        public async Task<IQueryable<PromotionMapping>> GetPromotionMappings()
        {
            return await Task.FromResult(_promotionMappingRepository.GetAll());
        }

        public Task<IQueryable<PromotionMapping>> GetPromotionMappingsByOrderID(int id)
        {
            return Task.FromResult(_promotionMappingRepository.GetAll().Where(e => e.OrderID == id));
        }

        public async Task InsertPromotionMapping(PromotionMapping promotionMapping)
        {
            await _promotionMappingRepository.InsertAsync(promotionMapping);
        }

        public async Task UpdatePromotionMapping(PromotionMapping promotionMapping)
        {
            await _promotionMappingRepository.UpdateAsync(promotionMapping);
        }
    }
}
