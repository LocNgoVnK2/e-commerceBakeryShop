using Infrastructure.Entities;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public interface IRateService
    {
        Task<IQueryable<Rate>> GetRates();

        Task<IQueryable<Rate>> GetRatesByProductID(int productId);
        Task<Rate> GetRate(int id);
        Task InsertRate(Rate rate);
        Task UpdateRate(Rate rate);
        Task DeleteRate(Rate rate);
    }

    public class RateService : IRateService
    {
        private IRateRepository ratesRepository;

        public RateService(IRateRepository rateRepository)
        {
            this.ratesRepository = rateRepository;
        }

        public async Task<IQueryable<Rate>> GetRates()
        {
            return await Task.FromResult(ratesRepository.GetAll());
        }

        public async Task<Rate> GetRate(int id)
        {
            return await ratesRepository.GetByIdAsync(id);
        }

        public async Task InsertRate(Rate rate)
        {
            await ratesRepository.InsertAsync(rate);

        }

        public async Task UpdateRate(Rate rate)
        {
            await ratesRepository.UpdateAsync(rate);

        }

        public async Task DeleteRate(Rate rate)
        {
            await ratesRepository.DeleteAsync(rate);

        }
        public async Task<IQueryable<Rate>> GetRatesByProductID(int productId)
        {
            IQueryable<Rate> rates =  ratesRepository.GetAll().Select(e=>e).Where(e=>e.ProductId == productId && e.StatusRate==true);
            return await Task.FromResult(rates);
        }
    }
}
