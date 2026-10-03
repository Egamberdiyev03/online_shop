using Application.DTOs.Product;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using DataAccess.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IRepository<Product> productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductDto> CreateProduct (CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.CreatedAt = DateTime.UtcNow;

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ResponseModel<List<ProductDto>>> GetAll()
        {
            var products = await _productRepository.GetAllAsync();

            if (!products.Any())
                return new($"Product topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<List<ProductDto>>(products));
        }

        public async Task<ResponseModel<PagedResult<ProductDto>>> GetPagedProducts(
            int pageNumber = 1, 
            int pageSize = 12, 
            string? search = null, 
            int? categoryId = null, 
            int? branchId = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 12;

            var query = _productRepository.GetAsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(s) || (p.Description != null && p.Description.ToLower().Contains(s)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(p => p.CompanyBranchId == branchId.Value);
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<List<ProductDto>>(products);

            var result = new PagedResult<ProductDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new(result);
        }

        public async Task<ResponseModel<ProductDto>> GetById (int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return new($"Product {id} topilmadi", HttpStatusCode.NotFound);

            return new(_mapper.Map<ProductDto>(product));
        }

        public async Task<ResponseModel<ProductDto>> UpdateProduct (UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.Id);

            if (product == null)
                return new($"Product {dto.Id} topilmadi", HttpStatusCode.NotFound);

            _mapper.Map(dto, product);
            await _productRepository.SaveChangesAsync();
                
            return new(_mapper.Map<ProductDto>(product));
        } 

        public async Task<bool> Deleteproduct (int id)
        {
           return await _productRepository.DeleteAsync(id);
        }
    }
}
