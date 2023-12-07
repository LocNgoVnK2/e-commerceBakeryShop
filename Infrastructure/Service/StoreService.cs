using Infrastructure.Entities;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public interface IStoreService
    {
        Task<IQueryable<Store>> GetStores();
        Task<Store> GetStore(int id);
        Task InsertStore(Store store);
        Task UpdateStore(Store store);
        Task DeleteStore(Store store);
    }
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _StoresRepository;
        public StoreService(IStoreRepository StoreRepository)
        {
            _StoresRepository = StoreRepository;
        }
        public async Task DeleteStore(Store store)
        {
            store.IsUsed = false;
            await _StoresRepository.UpdateAsync(store);
        }

        public async Task<Store> GetStore(int id)
        {
            return await _StoresRepository.GetByIdAsync(id);
        }

        public async Task<IQueryable<Store>> GetStores()
        {
            return await Task.FromResult(_StoresRepository.GetAll());
        }

        public async Task InsertStore(Store store)
        {
            await _StoresRepository.InsertAsync(store);
        }

        public async Task UpdateStore(Store store)
        {
            await _StoresRepository.UpdateAsync(store);
        }
    }
 
}
