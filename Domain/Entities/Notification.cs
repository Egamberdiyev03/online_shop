using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        // Kimga
        public int UserId { get; set; } = default!;

        // Nima haqida
        public NotificationType Type { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        // Holat
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(5);
        public DateTime? ReadAt { get; set; }
        public NotificationEntityType? RelatedEntityType { get; set; } // Order, Product, Branch
        public long? RelatedEntityId { get; set; }                     // masalan OrderId
        public string? Link { get; set; }                              // frontend route, masalan /orders/15

        // Kontekst
        public int? BranchId { get; set; }          // qaysi filialga tegishli (xodimlar uchun filtr)
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    }
}
