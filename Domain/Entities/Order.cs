using Domain.Enums;
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
        public int UserId { get; set; }
        public int CompanyBranchId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new();
        public User User { get; set; }
        public Payment Payment { get; set; }
        public CompanyBranch CompanyBranch { get; set; }
    }
}
