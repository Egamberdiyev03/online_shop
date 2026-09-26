using Application.DTOs.Customer;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto> CreateCustomer(CreateCustomerDto dto);
        Task<List<CustomerDto>> GetAllCustomer();
        Task<ResponseModel<CustomerDto>> GetCustomerById(int id);
        Task<ResponseModel<CustomerDto>> UpdateCustomer(UpdateCustomerDto dto);
        Task<ResponseModel<bool>> DeleteCustomer(int id);
    }
}
