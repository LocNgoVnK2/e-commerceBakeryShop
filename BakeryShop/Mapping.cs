﻿using AutoMapper;
using BakeryShop.Models;
using Infrastructure.Entities;

namespace BakeryShop
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            // account
            CreateMap<Accounts, AccountsViewModel>();
            CreateMap<AccountsViewModel, Accounts>();
            // Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryViewModel, Category>();
            // Product
            CreateMap<Product, ProductViewModel>();
            CreateMap<ProductViewModel, Product>();

            CreateMap<Accounts, AccountManagementViewModel>();
            CreateMap<AccountManagementViewModel, Accounts>();

            CreateMap<Employee, AccountManagementViewModel>();
            CreateMap<AccountManagementViewModel, Employee>();

            CreateMap<Employee, AccountManagementViewModel>();
            CreateMap<AccountManagementViewModel, Employee>();

            CreateMap<Models.PaymentResponseModel, Infrastructure.Entities.PaymentResponseModel>();
            CreateMap<Infrastructure.Entities.PaymentResponseModel, Models.PaymentResponseModel>();

            CreateMap<Store, StoreViewModel>();
            CreateMap<StoreViewModel, Store>();

            CreateMap<Slide, SlideViewModel>();
            CreateMap<SlideViewModel, Slide>();

            CreateMap<PromotionViewModel, Promotion>();
            CreateMap<Promotion, PromotionViewModel>();
        }
    }
}
