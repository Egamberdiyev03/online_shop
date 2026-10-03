using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public int BranchId { get; set; }

    }
}
