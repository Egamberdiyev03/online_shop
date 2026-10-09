using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public  class RejectPaymentDto
    {
        public int PaymentId { get; set; }
        public string Reason { get; set; }
    }
}
