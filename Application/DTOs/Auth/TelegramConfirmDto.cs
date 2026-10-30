using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class TelegramConfirmDto
    {
        public long TelegramChatId { get; set; }
        public string Code { get; set; }
    }
}
