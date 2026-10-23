using Application.DTOs.CompanyBranch;
using Application.DTOs.Product;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OnlineShop.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public class CompanyBranchController  : BaseController
    {
        private readonly ICompanyBranchService _service;
        public CompanyBranchController(ICompanyBranchService service )
        {
            _service = service;
        }

        // Token'dan BranchId olish uchun yordamchi metod
        private int? GetCurrentUserBranchId()
        {
            var branchIdClaim = User.FindFirst("companyBranchId")?.Value;
            if (int.TryParse(branchIdClaim, out int branchId))
                return branchId;
            return null;
        }

        // Token'dan rolni olish
        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost("Create")]
        public async Task<CompanyBranchDto> CreateCompanyBranch (CreateCompanyBranchDto dto)
        {
           return await _service.CreateCompanyBranch(dto);
        }

        // Filiallar ro'yxati Customer uchun ham ochiq (buyurtma berish va savat uchun kerak)
        [HttpGet("GetAll")]
        public async Task<List<CompanyBranchDto>> GetAllCompanyBranch ()
        {
           return await _service.GetAllCompanyBranchesAsync();
        }

        // BranchManager o'z filialini ko'ra oladi
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
        [HttpGet("GetById")]
        public async Task<ResponseModel<CompanyBranchDto>>GetCompanyBranch (int id)
        {
            var role = GetCurrentUserRole();

            // BranchManager faqat o'z filialini ko'ra oladi
            if (role == "BranchManager")
            {
                var userBranchId = GetCurrentUserBranchId();
                if (userBranchId != id)
                {
                    return new("Siz faqat o'z filialingiz ma'lumotlarini ko'ra olasiz.", System.Net.HttpStatusCode.Forbidden);
                }
            }

            return await _service.GetByIdCompanyBranch(id);
        }

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPut("Update")]
        public async Task<ResponseModel<CompanyBranchDto>> UpdateCompanyBranch (UpdateCompanyBranchDto dto)
        {
            return await _service.UpdateCompanyBranch(dto);
        }

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpDelete("Delete")]
        public async Task<ResponseModel<bool>> DeleteCompanyBranch (int id)
        {
            return await _service.DeleteCompanyBranch(id);
        }

        // BranchManager o'z filiali mahsulotlarini ko'ra oladi
        [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
        [HttpGet("GetCompanyBranchAllProduct")]
        public async Task<ActionResult<List<ProductDto>>> GetCompanyBranchAllProduct(int companybranchid)
        {
            var role = GetCurrentUserRole();

            // BranchManager faqat o'z filiali mahsulotlarini ko'ra oladi
            if (role == "BranchManager")
            {
                var userBranchId = GetCurrentUserBranchId();
                if (userBranchId != companybranchid)
                {
                    return Forbid();
                }
            }

            return await _service.GetCompanyBranchAllProduct(companybranchid);
        }


    }
}
