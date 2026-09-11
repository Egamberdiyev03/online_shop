using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }


        public Category Category { get; set; }
        public List<Comment> Comments { get; set; } = new();
        public Company_filial Company_Filial { get; set; }
    }
}
