using Application.DTOs.Customer;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpPost("Create")]
        public async Task<CustomerDto> CreateCustomer (CreateCustomerDto dto)
             => await _service.CreateCustomer(dto);  
        
        [HttpGet("GetById")]
        public async Task<ResponseModel<CustomerDto>> GetCustomerById (int id) 
              => await _service.GetCustomerById(id);
        
        [HttpGet("GetAll")]
        public async Task<List<CustomerDto>> GetAllCustomer()
              => await _service.GetAllCustomer();

        [HttpPut("Update")]
        public async Task<ResponseModel<CustomerDto>> UpdateCustomer(UpdateCustomerDto dto)
             =>   await _service.UpdateCustomer(dto);
        
        [HttpDelete("Delete")]
        public async Task<ResponseModel<bool>> DeleteCustomer(int id)
             => await _service.DeleteCustomer(id);
        

        
        
    }
}
