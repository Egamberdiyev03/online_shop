using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public class TelegramLoginCodeDto
    {
        [Required]
        [StringLength(128, MinimumLength = 32)]
        public string Code { get; set; } = null!;
    }
}
