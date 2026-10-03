using Application.DTOs.User;
using Application.Extentions;
using Application.Interfaces;
using Application.Mapping;
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
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IRepository<CompanyBranch> _branchRepository;
        private readonly IMapper _mapper;

        public UserService(IRepository<User> repository, IMapper mapper, IRepository<CompanyBranch> branchRepository = null)
        {
            _repository = repository;
            _mapper = mapper;
            _branchRepository = branchRepository;
        }

        public async Task<UserDto> CreateUser (CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);   
        }

        public async Task<List<UserDto>> GetAllUser ()
        {
            var customers = await _repository.GetAsQueryable()
                .Select(c => new UserDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    Address = c.Address,
                    Email = c.Email,
                    Location = c.Location,
                    PhoneNumber = c.PhoneNumber,
                    Role = c.Role.ToString(),
                    IsEmailConfirmed = c.IsEmailConfirmed,
                    CompanyId = c.CompanyId,
                    CompanyBranchId = c.CompanyBranchId
                })
                .ToListAsync();
            return customers;
        }
  
        public async Task<ResponseModel<UserDto>> GetUserById (int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if(customer==null)
            return new($"Customer {id} topilmadi",HttpStatusCode.NotFound);

            var dto = _mapper.Map<UserDto>(customer);
            dto.Role = customer.Role.ToString();
            return new(dto);  
        }
        
        public async Task<ResponseModel<UserDto>> UpdateUser (UpdateUserDto dto)
        {
            var customer = await _repository.GetByIdAsync(dto.Id);
                
            if (customer == null)
                return new($"Customer topilmadi", HttpStatusCode.NotFound);
           
            _mapper.Map(dto, customer);
            await _repository.SaveChangesAsync();

            var resDto = _mapper.Map<UserDto>(customer);
            resDto.Role = customer.Role.ToString();
            return new(resDto);
        }

        public async Task<ResponseModel<bool>> DeleteUser(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted) return new("Customer topilmadi", HttpStatusCode.NotFound);
            return new(true);
        }

        public async Task<ResponseModel<bool>> AssignCompanyAdmin(int userId, int companyId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return new("Foydalanuvchi topilmadi", HttpStatusCode.NotFound);

            var allUsers = await _repository.GetAllAsync();
            var adminCount = allUsers.Count(u => u.CompanyId == companyId && u.Role == Domain.Enums.Role.CompanyAdmin);
            
            if (adminCount >= 3)
            {
                return new("Kompaniyaga maksimal adminlar soni (3 ta) yetib kelgan!", HttpStatusCode.BadRequest);
            }

            user.Role = Domain.Enums.Role.CompanyAdmin;
            user.CompanyId = companyId;
            user.CompanyBranchId = null;

            await _repository.SaveChangesAsync();
            return new(true);
        }

        public async Task<ResponseModel<bool>> RemoveCompanyAdmin(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return new("Foydalanuvchi topilmadi", HttpStatusCode.NotFound);

            user.Role = Domain.Enums.Role.Customer;
            user.CompanyId = null;
            user.CompanyBranchId = null;

            await _repository.SaveChangesAsync();
            return new(true);
        }

        public async Task<ResponseModel<bool>> AssignBranchManager(int userId, int branchId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return new("Foydalanuvchi topilmadi", HttpStatusCode.NotFound);

            int? compId = null;
            if (_branchRepository != null)
            {
                var branch = await _branchRepository.GetByIdAsync(branchId);
                if (branch == null) return new("Filial topilmadi", HttpStatusCode.NotFound);
                compId = branch.CompanyId;
            }

            user.Role = Domain.Enums.Role.BranchManager;
            user.CompanyBranchId = branchId;
            if (compId.HasValue) user.CompanyId = compId.Value;

            await _repository.SaveChangesAsync();
            return new(true);
        }

        public async Task<ResponseModel<bool>> RemoveBranchManager(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null) return new("Foydalanuvchi topilmadi", HttpStatusCode.NotFound);

            user.Role = Domain.Enums.Role.Customer;
            user.CompanyBranchId = null;

            await _repository.SaveChangesAsync();
            return new(true);
        }

        public async Task<ResponseModel<List<UserDto>>> GetUsersByBranchId(int branchId)
        {
            var users = await _repository.GetAsQueryable()
                .Where(u => u.CompanyBranchId == branchId)
                .Select(c => new UserDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    Address = c.Address,
                    Email = c.Email,
                    Location = c.Location,
                    PhoneNumber = c.PhoneNumber,
                    Role = c.Role.ToString(),
                    CompanyId = c.CompanyId,
                    CompanyBranchId = c.CompanyBranchId,
                    IsEmailConfirmed = c.IsEmailConfirmed
                })
                .ToListAsync();

            return new(users);
        }
    }
}
