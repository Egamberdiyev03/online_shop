using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }


        public List<Order> Orders { get; set; } 
        public Cart Cart { get; set; }  
        public List<Comment> Comments { get; set; }

    }
}
