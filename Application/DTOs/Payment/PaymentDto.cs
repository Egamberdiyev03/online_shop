using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
