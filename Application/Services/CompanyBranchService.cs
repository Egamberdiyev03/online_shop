using Application.DTOs.CompanyBranch;
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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CompanyBranchService : ICompanyBranchService
    {
        private readonly IRepository<CompanyBranch> _branchRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;

        public CompanyBranchService(
            IRepository<CompanyBranch> branchRepository,
            IRepository<Product> productRepository,
            IMapper mapper
            )
        {
            _branchRepository = branchRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }
        public async Task<CompanyBranchDto> CreateCompanyBranch(CreateCompanyBranchDto dto)
        {
            var combranch = new CompanyBranch
            {
                Address = dto.Address,
                CompanyId = dto.CompanyId,
                PhoneNumber = dto.PhoneNumber,
                Name = dto.Name,
                Location = dto.Location
            };
            await _branchRepository.AddAsync(combranch);
            await _branchRepository.SaveChangesAsync();
            return new CompanyBranchDto
            {
                Id = combranch.Id,
                Address = combranch.Address,
                Name = combranch.Name,
                CompanyId = combranch.CompanyId,
                PhoneNumber = combranch.PhoneNumber,
                Location = combranch.Location
            };
        }

        public async Task<List<CompanyBranchDto>> GetAllCompanyBranchesAsync()
        {
            var combranches = await _branchRepository.GetAsQueryable().
                Select(c => new CompanyBranchDto
                {
                    Id = c.Id,
                    Address = c.Address,
                    Name = c.Name,
                    Location = c.Location,
                    CompanyId = c.CompanyId,
                    PhoneNumber = c.PhoneNumber
                }).ToListAsync();

            return combranches;
        }

        public async Task<ResponseModel<CompanyBranchDto>> GetByIdCompanyBranch (int id)
        {
            var combranch = await _branchRepository.GetByIdAsync(id);
            if (combranch == null)
                return new($"CompanyBranch {id} topilmadi",System.Net.HttpStatusCode.NotFound);

            return new(new CompanyBranchDto
            {
                Id = combranch.Id,
                Address = combranch.Address,
                Name = combranch.Name,
                PhoneNumber = combranch.PhoneNumber,
                CompanyId = combranch.CompanyId,
                Location = combranch.Location   
            });
        }
        
        public async Task<ResponseModel<CompanyBranchDto>> UpdateCompanyBranch (UpdateCompanyBranchDto dto)
        {
            var combranch = await _branchRepository.GetByIdAsync(dto.Id);

            if (combranch == null)
                return new($"CompanyBranch {dto.Id} topilmadi",System.Net.HttpStatusCode.NotFound);

            _mapper.Map(dto, combranch);

            await _branchRepository.SaveChangesAsync();

            return new(_mapper.Map<CompanyBranchDto>(combranch));
        }

        public async Task<ResponseModel<bool>> DeleteCompanyBranch (int id)
        {
            return new(await _branchRepository.DeleteAsync(id));
          
        }

        public async Task<List<ProductDto>> GetCompanyBranchAllProduct(int companybranchid)
        {
          var products =  await  _productRepository.GetAsQueryable().Where(c => c.CompanyBranchId == companybranchid).ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }
    }
}
