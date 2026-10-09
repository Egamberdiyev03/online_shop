using Application.DTOs.Company;
using Application.DTOs.CompanyBranch;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _service;

        public CompanyController(ICompanyService service)
        {
            _service = service;
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Create")]
        public async Task<CompanyDto> CreateCompany (CreateCompanyDto dto)
        {
           return await _service.CreateCompany(dto);
        }

        [HttpGet("GetById")]
        public async Task<ResponseModel<CompanyDto>> GetCompanyById(int id)
        {
           return await _service.GetCompanyById(id);
        }

        [HttpGet("GetAll")]
        public async Task<List<CompanyDto>> GetAllCompany()
        {
            var companies = await _service.GetAllCompanies();
            return companies;
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("Update")]
        public async Task<ResponseModel<CompanyDto>> UpdateCompany(UpdateCompanyDto dto)
        {
          return await  _service.UpdateCompany(dto);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("Delete")]
        public async Task<ResponseModel<bool>> DeleteCompany (int id)
        {
           return await  _service.DeleteCompany(id);
        }

        [HttpGet("GetBranchesByCompanyId")]
        public  async Task<ResponseModel<List<CompanyBranchDto>>> GetBranchesByCompanyId(int companyId)
        {
           return await _service.GetBranchesByCompanyId(companyId);
        }


    }
}
