using Application.DTOs.Company;
using Application.DTOs.CompanyBranch;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _service;
        public CompanyController(ICompanyService service)
        {
            _service = service;
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Create")]
        public async Task<CompanyDto> CreateCompany (CreateCompanyDto dto)
            => await _service.CreateCompany(dto);
        
        [HttpGet("GetById")]
        public async Task<ResponseModel<CompanyDto>> GetCompanyById(int id)
            => await _service.GetCompanyById(id);

        [HttpGet("GetAll")]
        public async Task<List<CompanyDto>> GetAllCompany()
            => await _service.GetAllCompanies();
           
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("Update")]
        public async Task<ResponseModel<CompanyDto>> UpdateCompany(UpdateCompanyDto dto)
            => await  _service.UpdateCompany(dto);

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("Delete")]
        public async Task<ResponseModel<bool>> DeleteCompany (int id)
            => await  _service.DeleteCompany(id);

        [HttpGet("GetBranchesByCompanyId")]
        public  async Task<ResponseModel<List<CompanyBranchDto>>> GetBranchesByCompanyId(int companyId)
            =>  await _service.GetBranchesByCompanyId(companyId);
    }
}
