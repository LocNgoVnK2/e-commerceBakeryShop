using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace BakeryShop.Controllers
{
    public class StoreController : Controller

    {
        private readonly IStoreService _storeService;
        private readonly IMapper _mapper;
        public StoreController(IStoreService storeService, IMapper mapper)
        {
            _storeService = storeService;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult> AddStore(StoreViewModel model)
        {

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
         
                try
                { 
                    Store store = _mapper.Map<Store>(model);

                    await _storeService.InsertStore(store);
                    
                    scope.Complete();

                    return RedirectToAction("StoreManagement", "DashBoard");

                }
                catch (Exception ex)
                {

                    scope.Dispose();
                    return BadRequest(ex.Message);
                }
            }
        }

        //EditStore

        [HttpPost]
        public async Task<ActionResult> EditStore(StoreViewModel model)
        {

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                try
                {
                    Store store = _mapper.Map<Store>(model);
                    store.IsUsed = true;
                    await _storeService.UpdateStore(store);

                    scope.Complete();

                    return RedirectToAction("StoreManagement", "DashBoard");

                }
                catch (Exception ex)
                {

                    scope.Dispose();
                    return BadRequest(ex.Message);
                }
            }
        }

        public async Task<ActionResult> DeleteStore(int id)
        {

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {

                try
                {
                    Store store =  await _storeService.GetStore(id);
                    await _storeService.DeleteStore(store);

                    scope.Complete();

                    return RedirectToAction("StoreManagement", "DashBoard");

                }
                catch (Exception ex)
                {

                    scope.Dispose();
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
