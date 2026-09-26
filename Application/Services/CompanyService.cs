using Application.DTOs.Company;
using Application.DTOs.CompanyBranch;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public  class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<CompanyBranch> _companyBranchRepository;
        private readonly IMapper _mapper;

        public CompanyService(
            IRepository<Company> companyrepository,
            IRepository<CompanyBranch> companyBranchRepository,
            IMapper mapper)
        {
            _companyRepository = companyrepository;
            _companyBranchRepository = companyBranchRepository;
            _mapper = mapper;
        }

        public async Task<CompanyDto> CreateCompany (CreateCompanyDto dto)
        {
           var company = _mapper.Map<Company>(dto);
             await  _companyRepository.AddAsync(company);
             await _companyRepository.SaveChangesAsync();

            return _mapper.Map<CompanyDto>(company);
        }

        public async Task<ResponseModel<CompanyDto>> GetCompanyById(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null)
                return new($"Company topilmadi (ID: {id})", HttpStatusCode.NotFound);

            return new(_mapper.Map<CompanyDto>(company));
        }

        public async Task<List<CompanyDto>> GetAllCompanies()
        {
            var companies = await _companyRepository.GetAllAsync();

            return _mapper.Map<List<CompanyDto>>(companies);
        }

        public async Task<ResponseModel<CompanyDto>> UpdateCompany( UpdateCompanyDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(dto.Id);

            if (company == null)
                return new($"Company topilmadi (ID: {dto.Id})", HttpStatusCode.NotFound);

            _mapper.Map(dto, company);   

            await _companyRepository.SaveChangesAsync();

            return new(_mapper.Map<CompanyDto>(company));
        }

        public async Task<ResponseModel<bool>> DeleteCompany(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null)
                return new($"Company topilmadi (ID: {id})", HttpStatusCode.NotFound);

            await _companyRepository.DeleteAsync(id);
            await _companyRepository.SaveChangesAsync();

            return new(true);
        }

        public async Task<ResponseModel<List<CompanyBranchDto>>> GetBranchesByCompanyId(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
                return new("Bu company mavjud emas", HttpStatusCode.BadRequest);

            var companyBranches = await _companyBranchRepository.GetAsQueryable()
                .Where(d=>d.CompanyId==companyId)
                .ToListAsync();

            if (!companyBranches.Any())
                return new($"bu company branchlari mavjud emas", HttpStatusCode.NotFound);

            return new(_mapper.Map<List<CompanyBranchDto>>(companyBranches));  
           
        }
    }
}
