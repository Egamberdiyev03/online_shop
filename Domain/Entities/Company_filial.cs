using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Company_filial
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int ProductId { get; set; }
        public int CompanyId { get; set; }

        public List<Product> Products { get; set; }
        public Company Company { get; set; }
    }
}
