using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System;
using System.Transactions;
using System.Security.Principal;

namespace BakeryShop.Controllers
{
    public class AccountManagementController : Controller
    {
        private readonly IAccountsService _accountsService;
        private readonly IEmployeeService _employeeService;
        public AccountManagementController(IAccountsService accountsService, IEmployeeService employeeService) { 
            _accountsService = accountsService;
            _employeeService = employeeService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddUserAccount()
        {
            return RedirectToAction("AddUserAccount", "DashBoard");
        }
        [HttpPost]
        public async Task<ActionResult> AddAccount(AccountManagementViewModel model)
        {
          
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                IQueryable<Accounts> accounts = await _accountsService.GetAccounts();
                bool accExist = accounts.Any(element => element.Username == model.Username);
                bool emailExist = accounts.Any(element => element.Email == model.Email);
                if (accExist)
                {
                    TempData["ErrorMessage"] = "Tên tài khoản đã tồn tại";
                    return RedirectToAction("AddUserAccount");
                }else if (emailExist)
                {

                    TempData["ErrorMessage"] = "Email đã tồn tại";
                    return RedirectToAction("AddUserAccount");
                }
                try
                {
                    
                    Employee newEmployee = new Employee()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        Position = model.Position
                    };
                    await _employeeService.InsertEmployee(newEmployee);

                    Accounts newAccount = new Accounts()
                    {
                        Email = model.Email,
                        Username = model.Username,
                        Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                        Role = "2",
                        EmployeeID = newEmployee.EmployeeID,
                        IsActivate = true,
                        IdStore = model.IdStore
                    };
                    await _accountsService.InsertAccount(newAccount);
                    scope.Complete(); 

                    return RedirectToAction("AccountManagement", "DashBoard");

                }
                catch (Exception ex)
                {
                  
                    scope.Dispose();
                    return BadRequest(ex.Message);
                }
            }
        }

        public async Task<ActionResult> DeleteAccount(int id)
        {
            try
            {

                Accounts existingAccounts = await _accountsService.GetAccount(id);
                await _accountsService.DeleteAccount(existingAccounts);
                return RedirectToAction("AccountManagement", "DashBoard");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult> UpdateAccount(AccountManagementViewModel model)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    Employee employeeExist = await _employeeService.GetEmployee((int)model.EmployeeID);
                    Accounts accountExist = await _accountsService.GetAccount((int)model.AccountID);

                    accountExist.Email = model.Email;
                    accountExist.Username = model.Username;
                    accountExist.IdStore = model.IdStore;
                    if (model.Password != null)
                    {
                        accountExist.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    }
                    employeeExist.FirstName = model.FirstName;
                    employeeExist.LastName = model.LastName;
                    employeeExist.PhoneNumber = model.PhoneNumber;
                    employeeExist.Position = model.Position;
                    

                    await _employeeService.UpdateEmployee(employeeExist);
                    await _accountsService.UpdateAccount(accountExist);

                    scope.Complete();
                    return RedirectToAction("AccountManagement", "DashBoard");
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
