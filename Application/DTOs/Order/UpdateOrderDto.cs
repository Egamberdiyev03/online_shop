using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public class UpdateOrderDto
    {
        public OrderStatus Status { get; set; }
    }
}
