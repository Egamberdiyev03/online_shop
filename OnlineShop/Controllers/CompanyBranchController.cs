using Application.DTOs.CompanyBranch;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyBranchController  :ControllerBase
    {
        private readonly ICompanyBranchService _service;
        public CompanyBranchController(ICompanyBranchService service )
        {
            _service = service;
        }

        [HttpPost("Create")]
        public async Task<CompanyBranchDto> CreateCompanyBranch (CreateCompanyBranchDto dto)
        {
           return await _service.CreateCompanyBranch(dto);
        }

        [HttpGet("GetAll")]
        public async Task<List<CompanyBranchDto>> GetAllCompanyBranch ()
        {

           return await _service.GetAllCompanyBranchesAsync();
        }

        [HttpGet("GetById")]
        public async Task<ResponseModel<CompanyBranchDto>>GetCompanyBranch (int id)
        {
          return await  _service.GetByIdCompanyBranch(id);
        }

        [HttpPut("Update")] 
        public async Task<ResponseModel<CompanyBranchDto>> UpdateCompanyBranch (UpdateCompanyBranchDto dto)
        {
            return await _service.UpdateCompanyBranch(dto);
        }

        [HttpDelete("Delete")]
        public async Task<ResponseModel<bool>> DeleteCompanyBranch (int id)
        {
            return await _service.DeleteCompanyBranch(id);
        }
      
    } 
}
