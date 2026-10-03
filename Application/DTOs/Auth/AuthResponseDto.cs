using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int? CompanyId { get; set; }
        public int? CompanyBranchId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
