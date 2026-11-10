using Application.DTOs.Product;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using DataAccess.Filter;
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
            product.CreatedAt = DateTime.UtcNow.AddHours(5);

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

        public async Task<ResponseModel<PagedResult<ProductDto>>> GetPagedProducts(PagedQuery q)
        {    
            if(q.PageNumber < 1)  q.PageNumber = 1;
            if (q.PageSize < 1)  q.PageSize = 12;

            var query = _productRepository.GetAsQueryable();

            if (q.Filters != null && q.Filters.Any())
            {
                query = query.ApplyFilters(q.Filters);
            }

            if(!string.IsNullOrWhiteSpace(q.SortBy))
            {
                query = query.ApplySorting(q.SortBy, q.IsDescending);
            }
            else
            {
                query = query.OrderByDescending(p => p.Id);
            }

            var paged = await query.ToPagedListAsync(q.PageNumber, q.PageSize);

            var items = _mapper.Map<List<ProductDto>>(paged.Items);

            var result = new PagedResult<ProductDto>
            {
                Items = items,
                TotalCount = paged.TotalCount,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize
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
