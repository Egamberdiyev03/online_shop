using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string?  ChekUrl { get; set; }
        public string? TransactionNote { get; set; } // mijoz qoldirgan izoh
        public int?  ConfirmedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public PaymentStatus Status { get; set; }
        public string? RejectionReason { get; set; } // agar chek soxta bolsa rad etish sababi
        public Order Order { get; set; }
    }
}
