using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.DTOs.Comment
{
    public class CreateCommentDto
    {
        public string Content { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
    }
}
