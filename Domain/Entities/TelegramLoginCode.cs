using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TelegramLoginCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CodeHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
    }
}
