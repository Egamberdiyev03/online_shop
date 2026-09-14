using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.DTOs.Comment
{
    public class UpdateCommentDto
    {
        public string Content { get; set; }
        public int CustomerId { get; set; }
        public string ProductId { get; set; }
    }
}
