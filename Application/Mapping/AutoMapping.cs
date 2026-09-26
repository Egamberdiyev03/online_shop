using Application.DTOs.Comment;
using Application.DTOs.Company;
using Application.DTOs.CompanyBranch;
using Application.DTOs.Customer;
using Application.DTOs.Order;
using Application.DTOs.OrderItem;
using Application.DTOs.Payment;
using Application.DTOs.Product;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<CreateCompanyDto, Company>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();

            CreateMap<CompanyBranch, CreateCompanyBranchDto>().ReverseMap();
            CreateMap<CompanyBranch, CompanyBranchDto>().ReverseMap();
            CreateMap<UpdateCompanyBranchDto, CompanyBranch>();

            CreateMap<CustomerDto, Customer>().ReverseMap();
            CreateMap<CreateCustomerDto, Customer>().ReverseMap();
            CreateMap<UpdateCustomerDto, Customer>().ReverseMap();

            CreateMap<CreateOrderDto, Order>().ReverseMap();
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();

            CreateMap<CreateProductDto, Product>().ReverseMap();
            CreateMap<CreateProductDto, ProductDto>().ReverseMap();
            CreateMap<ProductDto, Product>().ReverseMap();
            CreateMap<UpdateProductDto, ProductDto>().ReverseMap();
            CreateMap<UpdateProductDto, Product>().ReverseMap();

            CreateMap<CreatePaymentDto, Payment>().ReverseMap();

            CreateMap<CreateCommentDto, Comment>();
            CreateMap<CommentDto, Comment>().ReverseMap(); 

        }
    }
}
