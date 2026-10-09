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
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        // Status: Pending (0), Completed (1), Failed (2), Cancelled (4)
        public PaymentStatus Status { get; set; }

        // To'lov usuli: "CardTransfer" (Karta orqali), "Cash" (Naqd)
        public string? PaymentMethod { get; set; }

        // Yuklangan chek rasm URL-i yoki yo'li
        public string? ChekUrl { get; set; }

        // Mijoz kiritgan tranzaksiya ma'lumoti (masalan: "9860****1234 kartasidan o'tkazildi")
        public string? TransactionNote { get; set; }


        // Kim tasdiqladi (Admin / Mudir) va qachon
        public int? ConfirmedByUserId { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        // Agar admin rad etsa, sababi
        public string? RejectionReason { get; set; }
    }
}
