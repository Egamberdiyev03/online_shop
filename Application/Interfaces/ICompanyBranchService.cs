using Application.DTOs.CompanyBranch;
using Application.DTOs.Product;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
     public interface ICompanyBranchService
    {
        Task<CompanyBranchDto> CreateCompanyBranch(CreateCompanyBranchDto dto);
        Task<List<CompanyBranchDto>> GetAllCompanyBranchesAsync();
        Task<ResponseModel<CompanyBranchDto>> GetByIdCompanyBranch(int id);
        Task<ResponseModel<CompanyBranchDto>> UpdateCompanyBranch(UpdateCompanyBranchDto dto);
        Task<ResponseModel<bool>> DeleteCompanyBranch(int id);


        Task<List<ProductDto>> GetCompanyBranchAllProduct(int companybranchid);  
    }
}
