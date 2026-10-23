using Application.DTOs.User;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace OnlineShop.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public class UserController : BaseController
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("Create")]
        public async Task<UserDto> CreateUser (CreateUserDto dto)
             => await _service.CreateUser(dto);  
        
        [HttpGet("GetById")]
        public async Task<ResponseModel<UserDto>> GetUserById (int id) 
              => await _service.GetUserById(id);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet("GetAll")]
        public async Task<List<UserDto>> GetAllUser()
              => await _service.GetAllUser();

        [HttpPut("Update")]
        public async Task<ResponseModel<UserDto>> UpdateUser(UpdateUserDto dto)
             =>   await _service.UpdateUser(dto);


        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpDelete("Delete")]
        public async Task<ResponseModel<bool>> DeleteUser(int id)
             => await _service.DeleteUser(id);

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("AssignCompanyAdmin")]
        public async Task<ResponseModel<bool>> AssignCompanyAdmin(int userId, int companyId)
             => await _service.AssignCompanyAdmin(userId, companyId);
             
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("RemoveCompanyAdmin")]
        public async Task<ResponseModel<bool>> RemoveCompanyAdmin(int userId)
             => await _service.RemoveCompanyAdmin(userId);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost("AssignBranchManager")]
        public async Task<ResponseModel<bool>> AssignBranchManager(int userId, int branchId)
             => await _service.AssignBranchManager(userId, branchId);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost("RemoveBranchManager")]
        public async Task<ResponseModel<bool>> RemoveBranchManager(int userId)
             => await _service.RemoveBranchManager(userId);

        [HttpGet("GetUsersByBranchId")]
        public async Task<ResponseModel<List<UserDto>>> GetUsersByBranchId(int branchId)
             => await _service.GetUsersByBranchId(branchId);
    }
}
