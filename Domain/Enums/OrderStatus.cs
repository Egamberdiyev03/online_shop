using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        Pending,           // Kutilmoqda
        Confirmed,         // Tasdiqlangan
        Shipped,           // Jo'natildi
        Delivered,         // Yetkazildi 
        Cancelled          // Bekor qilindi
    }
}
