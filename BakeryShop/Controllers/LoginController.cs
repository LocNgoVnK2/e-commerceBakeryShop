﻿using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System;
using Newtonsoft.Json.Linq;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using AspNetCore.ReCaptcha;
using System.ComponentModel.DataAnnotations;

namespace BakeryShop.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccountsService accountsService;
        private readonly IEmailService emailService;
        private readonly IEmployeeService employeeService;
        private readonly IStoreService storeService;
        private readonly IMapper mapper;
        public LoginController(IEmployeeService employeeService,
                               IAccountsService accountsService,
                               IEmailService emailService,
                               IStoreService storeService,
                               IMapper mapper)
        {
            this.accountsService = accountsService;
            this.emailService = emailService;
            this.employeeService = employeeService;
            this.storeService = storeService;
            this.mapper = mapper;
        }
        
        public async Task<IActionResult> Admin()
        {

            return View("Login");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken] // chống request từ nơi ko phải site của mình bằng 1 cái token hidden
        [ValidateReCaptcha]
       
        public async Task<IActionResult> Login(AccountsViewModel accounts)
        {
            
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "reCAPTCHA validation failed. Bạn phải đánh dấu để kiểm tra không phải là robot.";
                return View(accounts);
            }
            

            if (accounts != null)
            {
               
                Accounts acc = mapper.Map<Accounts>(accounts);

                IQueryable<Accounts> list = await accountsService.GetAccounts();

                Accounts user = list.Where(e =>e.Username == accounts.Username).FirstOrDefault();
                if (user != null && BCrypt.Net.BCrypt.Verify(acc.Password, user.Password) && user.IsActivate ==true)
                {
                    Accounts loginUser = await list.Where(a => a.Username == user.Username).FirstOrDefaultAsync();
                    Employee employee = await employeeService.GetEmployee((int)loginUser.EmployeeID);
                    Store store = await storeService.GetStore((int)loginUser.IdStore);

                    AccountsViewModel accountViewModel = mapper.Map<AccountsViewModel>(loginUser);
                    accountViewModel.PhoneNumber = employee.PhoneNumber;
                    accountViewModel.FirstName = employee.FirstName;
                    accountViewModel.LastName = employee.LastName;
                    accountViewModel.Position = employee.Position;

                    accountViewModel.StoreName = store.StoreName;

                    string userJson = JsonConvert.SerializeObject(accountViewModel);
                    HttpContext.Session.SetString("LoggedInUser", userJson);
                    
                    return RedirectToAction("Index", "DashBoard");
                }
                else
                {

                    ViewBag.ErrorMessage = "Sai thông tin đăng nhập.";
                    return View();

                }

            }
            else
            {
                return NotFound();
            }
        }
     
        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("LoggedInUser");
            return RedirectToAction("Index", "Home");
        }
            public async Task<IActionResult> ForgotPassword(AccountsViewModel model)
        {

            return View(model);
        }
     
        [HttpPost]
        public async Task<IActionResult> SubmitChangePassword(AccountsViewModel model)
        {

            try
            {
                if (model.Password != model.ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu nhập lại không khớp.";
                    return RedirectToAction("ForgotPassword", model);
                }

                var listAccount = await accountsService.GetAccounts();
                Accounts account = await listAccount.Where(e => e.Username == model.Username && e.ValidationCode == model.ValidationCode).FirstOrDefaultAsync();
                if (account == null)
                {
                    TempData["ErrorMessage"] = "Thông tin bạn nhập không chính xác.";
                    return RedirectToAction("ForgotPassword", model);
                }
                else
                {
                    account.ValidationCode = null;
                    string newPass = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    account.Password = newPass;
                    await accountsService.UpdateAccount(account);
                    if (accountsService.UpdateAccount(account).IsCompletedSuccessfully)
                    {
                        TempData["ErrorMessage"] = "Cập nhật thành công.";
                        return RedirectToAction("ForgotPassword");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra.";
                        return View("ForgotPassword", model);
                    }
                }

                
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra.";
                return View("ForgotPassword", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendValidationCode(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Email Chưa được khai báo.";

                return RedirectToAction("ForgotPassword", "Login");
            }

            try
            {
                IQueryable<Accounts> list = await accountsService.GetAccounts();
                Accounts accountToSendMail =  list.FirstOrDefault(x=>x.Email==email);

                if (accountToSendMail != null && accountToSendMail.IsActivate==true)
                {
                    accountToSendMail.ValidationCode = GenerateRandomString(7);
                    await accountsService.UpdateAccount(accountToSendMail);
                    string subject = "Xin chào: " + accountToSendMail.Username + " | Email reset password";
                    string content = "Đây là email gửi tự động bởi hệ thống xác minh, Mã xác nhận của bận : " + accountToSendMail.ValidationCode;
                    var message = new EmailMessage(accountToSendMail.Email, subject, content);
                    emailService.SendEmail(message);

                    TempData["SuccessMessage"] = "Mã xác nhận đã được gửi thành công.";
                    return RedirectToAction("ForgotPassword", "Login");
                }else if(accountToSendMail == null)
                {

                    TempData["ErrorMessage"] = "Email chưa được khai báo.";
                    return RedirectToAction("ForgotPassword", "Login");
                }
                else
                {
                    TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa vĩnh viễn khỏi hệ thống.";
                    return RedirectToAction("ForgotPassword", "Login");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi gì đó.";
                return RedirectToAction("ForgotPassword", "Login");
            }
        }
        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
