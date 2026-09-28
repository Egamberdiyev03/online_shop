using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CompanyBranch
{
    public class CreateCompanyBranchDto   
    {
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string Address { get; set; }
        public int CompanyId { get; set; }
        public string Location { get; set; }
    }
}
