using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum PaymentStatus
    {
        Pending=0,     // Kutilmoqda — to'lov hali amalga oshirilmagan
        Completed=1,   // Muvaffaqiyatli — to'lov o'tdi
        Failed=2,      // Muvaffaqiyatsiz — karta rad etildi, mablag' yetarli emas va h.k.
        Refunded=3,    // Qaytarildi — mijozga pul qaytarilgan
        Cancelled =4   // Bekor qilindi — to'lov jarayoni tugallanmasdan to'xtatilgan
    }
}
