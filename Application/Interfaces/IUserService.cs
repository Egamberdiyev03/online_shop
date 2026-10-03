using Application.DTOs.User;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUser(CreateUserDto dto);
        Task<List<UserDto>> GetAllUser();
        Task<ResponseModel<UserDto>> GetUserById(int id);
        Task<ResponseModel<UserDto>> UpdateUser(UpdateUserDto dto);
        Task<ResponseModel<bool>> DeleteUser(int id);
        Task<ResponseModel<bool>> AssignCompanyAdmin(int userId, int companyId);
        Task<ResponseModel<bool>> RemoveCompanyAdmin(int userId);
        Task<ResponseModel<bool>> AssignBranchManager(int userId, int branchId);
        Task<ResponseModel<bool>> RemoveBranchManager(int userId);
        Task<ResponseModel<List<UserDto>>> GetUsersByBranchId(int branchId);
    }
}
