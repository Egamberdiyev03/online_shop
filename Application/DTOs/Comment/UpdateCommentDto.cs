using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Comment
{
    public class UpdateCommentDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
       public int StarRating { get; set; } = 0;
    }
}
