using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PasswordHash { get; set; }
        public Role Role { get; set; } = Role.Customer;
        public int? CompanyId { get; set; }
        public int? CompanyBranchId { get; set; }
        //email tasdiqlash
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationCode { get; set; }
        public DateTime? EmailConfirmationCodeExpiry { get; set; }


        public List<Order> Orders { get; set; } 
        public Cart Cart { get; set; }  
        public List<Comment> Comments { get; set; }

    }
}
