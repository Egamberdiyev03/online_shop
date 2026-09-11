using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public double TotalPrice { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
    }
}
