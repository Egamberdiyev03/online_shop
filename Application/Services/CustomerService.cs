using Application.DTOs.Customer;
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
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _repository;
        private readonly IMapper _mapper;

        public CustomerService(IRepository<Customer> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CustomerDto> CreateCustomer (CreateCustomerDto dto)
        {
            var customer = _mapper.Map<Customer>(dto);
            await _repository.AddAsync(customer);
            await _repository.SaveChangesAsync();
            return _mapper.Map<CustomerDto>(customer);   
        }

        public async Task<List<CustomerDto>> GetAllCustomer ()
        {
            var customers = await _repository.GetAsQueryable()
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    Address = c.Address,
                    Email = c.Email,
                    Location = c.Location,
                    PhoneNumber = c.PhoneNumber
                })
                .ToListAsync();
            return customers;
        }
  
        public async Task<ResponseModel<CustomerDto>> GetCustomerById (int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if(customer==null)
            return new($"Customer {id} topilmadi",HttpStatusCode.NotFound);

            return new(_mapper.Map<CustomerDto>(customer));  
        }
        
        public async Task<ResponseModel<CustomerDto>> UpdateCustomer (UpdateCustomerDto dto)
        {
            var customer = await _repository.GetByIdAsync(dto.Id);
                
            if (customer == null)
                return new($"Customer topilmadi", HttpStatusCode.NotFound);
           
            _mapper.Map(dto, customer);
            await _repository.SaveChangesAsync();

            return new(_mapper.Map<CustomerDto>(customer));
        }

        public async Task<ResponseModel<bool>> DeleteCustomer (int id)
        {
            var deleted= await  _repository.DeleteAsync(id);

            if (deleted == false)
                return new($"Customer {id} topilmadi", HttpStatusCode.NotFound);

            return new(true);   
        }
    }
}
