using Application.DTOs.Company;
using Application.DTOs.CompanyBranch;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyDto> CreateCompany(CreateCompanyDto dto);
        Task<ResponseModel<CompanyDto>> GetCompanyById(int id);
        Task<List<CompanyDto>> GetAllCompanies();
        Task<ResponseModel<CompanyDto>> UpdateCompany(UpdateCompanyDto dto);
        Task<ResponseModel<bool>> DeleteCompany(int id);

        Task<ResponseModel<List<CompanyBranchDto>>> GetBranchesByCompanyId(int companyId);
    }
}
